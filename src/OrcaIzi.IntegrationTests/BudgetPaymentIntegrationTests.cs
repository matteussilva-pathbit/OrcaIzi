using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Application.Interfaces.Services;
using OrcaIzi.Domain.Entities;
using OrcaIzi.Infrastructure.Context;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace OrcaIzi.IntegrationTests
{
    public class BudgetPaymentIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BudgetPaymentIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:Provider"] = "InMemory",
                        ["Database:ApplyMigrationsOnStartup"] = "false"
                    });
                });

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions));
                    services.RemoveAll(typeof(DbContextOptions<OrcaIziDbContext>));
                    services.RemoveAll(typeof(OrcaIziDbContext));

                    var sqlServerAssembly = typeof(SqlServerDbContextOptionsExtensions).Assembly;
                    var sqlServerDescriptors = services
                        .Where(d => (d.ImplementationType?.Assembly == sqlServerAssembly) || d.ServiceType.Assembly == sqlServerAssembly)
                        .ToList();
                    foreach (var d in sqlServerDescriptors)
                    {
                        services.Remove(d);
                    }

                    var inMemoryProvider = new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider();

                    services.AddDbContext<OrcaIziDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("BudgetPaymentTestDb");
                        options.UseInternalServiceProvider(inMemoryProvider);
                    });

                    services.RemoveAll(typeof(IPaymentGateway));
                    services.AddSingleton<IPaymentGateway, FakePaymentGateway>();
                });
            });

            _client = _factory.CreateClient();
        }

        private async Task<string> AuthenticateAsync()
        {
            var registerDto = new RegisterDto
            {
                Username = $"payuser_{Guid.NewGuid()}",
                Email = $"pay_{Guid.NewGuid()}@test.com",
                Password = "Password123!",
                ZipCode = "12345678",
                Street = "Test St",
                Number = "100",
                Neighborhood = "Test Neighborhood",
                City = "Test City",
                State = "TS"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);
            response.EnsureSuccessStatusCode();
            var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
            return userDto!.Token;
        }

        [Fact]
        public async Task GeneratePix_ThenWebhook_ShouldMarkBudgetPaid()
        {
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var customerResponse = await _client.PostAsJsonAsync("/api/Customers", new CreateCustomerDto
            {
                Name = "Customer Pay",
                Email = "custpay@test.com",
                Phone = "11999999999",
                Document = "12345678901",
                Address = "Addr"
            });
            customerResponse.EnsureSuccessStatusCode();
            var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerDto>();

            var budgetResponse = await _client.PostAsJsonAsync("/api/Budgets", new CreateBudgetDto
            {
                Title = "Budget Pay",
                Description = "Desc",
                CustomerId = customer!.Id,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                Items = new List<CreateBudgetItemDto>
                {
                    new CreateBudgetItemDto { Name = "Item 1", Description = null, Quantity = 1, UnitPrice = 10 }
                }
            });
            budgetResponse.EnsureSuccessStatusCode();
            var budget = await budgetResponse.Content.ReadFromJsonAsync<BudgetDto>();

            var pixResponse = await _client.PostAsync($"/api/Budgets/{budget!.Id}/payment/pix", null);
            pixResponse.EnsureSuccessStatusCode();
            var pix = await pixResponse.Content.ReadFromJsonAsync<PixPaymentDto>();
            pix!.ExternalId.Should().Be("mp_1");
            pix.Status.Should().Be("pending");

            _client.DefaultRequestHeaders.Authorization = null;
            var webhookResponse = await _client.PostAsJsonAsync("/api/Payments/mercadopago/webhook", new MercadoPagoWebhookDto
            {
                Data = new MercadoPagoWebhookDataDto { Id = "mp_1" },
                Type = "payment"
            });
            webhookResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, webhookResponse.StatusCode);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var getBudget = await _client.GetFromJsonAsync<BudgetDto>($"/api/Budgets/{budget.Id}");
            getBudget!.Status.Should().Be("Paid");
        }

        private class FakePaymentGateway : IPaymentGateway
        {
            public Task<PixPaymentDto> CreatePixPaymentAsync(Budget budget, Customer customer)
            {
                return Task.FromResult(new PixPaymentDto
                {
                    Provider = "MercadoPago",
                    ExternalId = "mp_1",
                    Status = "pending",
                    Amount = budget.TotalAmount,
                    QrCode = "code",
                    QrCodeBase64 = "base64",
                    TicketUrl = "https://ticket",
                    CreatedAt = DateTime.UtcNow
                });
            }

            public Task<PixPaymentDto> GetPaymentAsync(string externalPaymentId)
            {
                return Task.FromResult(new PixPaymentDto
                {
                    Provider = "MercadoPago",
                    ExternalId = externalPaymentId,
                    Status = "approved",
                    Amount = 10
                });
            }
        }
    }
}
