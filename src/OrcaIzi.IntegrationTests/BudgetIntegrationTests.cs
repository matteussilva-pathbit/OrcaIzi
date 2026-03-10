using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrcaIzi.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrcaIzi.Infrastructure.Context;
using System.Net.Http.Json;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace OrcaIzi.IntegrationTests
{
    public class BudgetIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        public BudgetIntegrationTests(WebApplicationFactory<Program> factory)
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
                    // Remove all DbContextOptions
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
                        options.UseInMemoryDatabase("BudgetTestDb");
                        options.UseInternalServiceProvider(inMemoryProvider);
                    });
                });
            });
            _client = _factory.CreateClient();
        }

        private async Task<string> AuthenticateAsync()
        {
            var registerDto = new RegisterDto
            {
                Username = $"user_{Guid.NewGuid()}",
                Email = $"user_{Guid.NewGuid()}@test.com",
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
            return userDto.Token;
        }

        [Fact]
        public async Task CreateBudget_WithNullItemDescription_ShouldSucceed()
        {
            // Arrange
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create Customer
            var customerDto = new CreateCustomerDto
            {
                Name = "Test Customer",
                Email = "customer@test.com",
                Phone = "11999999999",
                Document = "12345678901",
                Address = "Customer Address"
            };
            var customerResponse = await _client.PostAsJsonAsync("/api/Customers", customerDto);
            customerResponse.EnsureSuccessStatusCode();
            var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerDto>();

            // Create Budget
            var budgetDto = new CreateBudgetDto
            {
                Title = "Test Budget",
                Description = "Budget Description",
                CustomerId = customer.Id,
                ExpirationDate = DateTime.Now.AddDays(7),
                Items = new List<CreateBudgetItemDto>
                {
                    new CreateBudgetItemDto
                    {
                        Name = "Item 1",
                        Description = null, // Testing null description
                        Quantity = 1,
                        UnitPrice = 100.50m
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Budgets", budgetDto);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Status Code: {response.StatusCode}, Content: {content}");
            }
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdBudget = await response.Content.ReadFromJsonAsync<BudgetDto>();
            Assert.NotNull(createdBudget);
            Assert.Single(createdBudget.Items);
            Assert.Null(createdBudget.Items[0].Description);
        }

        [Fact]
        public async Task UpdateBudget_RemoveItemDescription_ShouldSucceed()
        {
            // Arrange
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create Customer
            var customerDto = new CreateCustomerDto { Name = "Test Customer 2", Email = "c2@test.com", Phone = "11999999999", Document = "12345678902", Address = "Addr" };
            var custResponse = await _client.PostAsJsonAsync("/api/Customers", customerDto);
            var customer = await custResponse.Content.ReadFromJsonAsync<CustomerDto>();

            // Create Budget with Description
            var budgetDto = new CreateBudgetDto
            {
                Title = "Budget Update Test",
                Description = "Desc",
                CustomerId = customer.Id,
                Items = new List<CreateBudgetItemDto>
                {
                    new CreateBudgetItemDto { Name = "Item A", Description = "Initial Desc", Quantity = 1, UnitPrice = 50 }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/Budgets", budgetDto);
            var createdBudget = await createResponse.Content.ReadFromJsonAsync<BudgetDto>();

            // Act - Update to remove description
            var updateDto = new CreateBudgetDto
            {
                Title = createdBudget.Title,
                Description = createdBudget.Description,
                CustomerId = createdBudget.CustomerId,
                Status = createdBudget.Status,
                ExpirationDate = createdBudget.ExpirationDate,
                Items = new List<CreateBudgetItemDto>
                {
                    new CreateBudgetItemDto
                    {
                        Name = "Item A",
                        Description = null, // Removing description
                        Quantity = 1,
                        UnitPrice = 50
                    }
                }
            };

            var updateResponse = await _client.PutAsJsonAsync($"/api/Budgets/{createdBudget.Id}", updateDto);

            // Assert
            if (!updateResponse.IsSuccessStatusCode)
            {
                var content = await updateResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Update Failed: {updateResponse.StatusCode}, {content}");
            }
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            
            // Verify fetch
            var getResponse = await _client.GetAsync($"/api/Budgets/{createdBudget.Id}");
            var fetchedBudget = await getResponse.Content.ReadFromJsonAsync<BudgetDto>();
            Assert.Null(fetchedBudget.Items[0].Description);
        }
    }
}
