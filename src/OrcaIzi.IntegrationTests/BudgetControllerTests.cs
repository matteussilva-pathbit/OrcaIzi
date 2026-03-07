using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Domain.Core;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace OrcaIzi.IntegrationTests
{
    public class BudgetControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public BudgetControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<string> AuthenticateAsync()
        {
            var registerDto = new RegisterDto
            {
                Email = $"budget_test_{Guid.NewGuid()}@example.com",
                Username = $"budgetuser_{Guid.NewGuid()}",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<UserDto>();
            return content!.Token;
        }

        private async Task<CustomerDto> CreateCustomerAsync()
        {
            var customerDto = new CreateCustomerDto
            {
                Name = $"Test Customer {Guid.NewGuid()}",
                Email = $"customer_{Guid.NewGuid()}@test.com",
                Phone = "123456789",
                Document = "12345678900",
                Address = "Test Address"
            };

            var response = await _client.PostAsJsonAsync("/api/customers", customerDto);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
        }

        [Fact]
        public async Task CreateBudget_ShouldReturnOk_WhenDataIsValid()
        {
            // Arrange
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var customer = await CreateCustomerAsync();

            var budgetDto = new CreateBudgetDto
            {
                Title = "Test Budget",
                Description = "Test Description",
                CustomerId = customer.Id,
                ExpirationDate = DateTime.Now.AddDays(7),
                Observations = "Test Observation",
                Items = new List<CreateBudgetItemDto>
                {
                    new CreateBudgetItemDto { Name = "Item 1", Description = "Desc 1", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/budgets", budgetDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var content = await response.Content.ReadFromJsonAsync<BudgetDto>();
            content.Should().NotBeNull();
            content.Title.Should().Be(budgetDto.Title);
            content.CustomerId.Should().Be(customer.Id);
            content.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateBudget_ShouldReturnOk_WhenDataIsValid()
        {
            // Arrange
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var customer = await CreateCustomerAsync();

            var budgetDto = new CreateBudgetDto
            {
                Title = "Original Budget",
                Description = "Original Description",
                CustomerId = customer.Id,
                ExpirationDate = DateTime.Now.AddDays(7),
                Items = new List<CreateBudgetItemDto>
                {
                    new CreateBudgetItemDto { Name = "Item 1", Quantity = 1, UnitPrice = 100 }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/budgets", budgetDto);
            var createdBudget = await createResponse.Content.ReadFromJsonAsync<BudgetDto>();

            var updateDto = new CreateBudgetDto
            {
                Title = "Updated Budget",
                Description = "Updated Description",
                CustomerId = customer.Id,
                ExpirationDate = DateTime.Now.AddDays(14),
                Items = new List<CreateBudgetItemDto>
                {
                    new CreateBudgetItemDto { Name = "Item 1 Updated", Quantity = 2, UnitPrice = 150 }
                }
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/budgets/{createdBudget!.Id}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedContent = await response.Content.ReadFromJsonAsync<BudgetDto>();
            updatedContent.Should().NotBeNull();
            updatedContent.Title.Should().Be(updateDto.Title);
            updatedContent.Items.Should().HaveCount(1);
            updatedContent.Items.First().Name.Should().Be("Item 1 Updated");
        }

        [Fact]
        public async Task GetAll_ShouldReturnPagedResult_WhenCalled()
        {
            // Arrange
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var customer = await CreateCustomerAsync();

            // Create a budget first
            var budgetDto = new CreateBudgetDto
            {
                Title = "Paged Budget",
                Description = "Description",
                CustomerId = customer.Id,
                ExpirationDate = DateTime.Now.AddDays(30),
                Items = new List<CreateBudgetItemDto>
                {
                    new CreateBudgetItemDto { Name = "Item 1", Description = "Desc 1", Quantity = 2, UnitPrice = 50 }
                }
            };
            await _client.PostAsJsonAsync("/api/budgets", budgetDto);

            // Act
            var response = await _client.GetAsync("/api/budgets?pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<PagedResult<BudgetDto>>();
            content.Should().NotBeNull();
            content.Items.Should().NotBeEmpty();
            content.TotalCount.Should().BeGreaterThan(0);
        }
    }
}
