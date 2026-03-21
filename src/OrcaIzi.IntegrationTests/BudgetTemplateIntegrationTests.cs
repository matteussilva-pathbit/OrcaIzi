﻿namespace OrcaIzi.IntegrationTests
{
    public class BudgetTemplateIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BudgetTemplateIntegrationTests(WebApplicationFactory<Program> factory)
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
                        options.UseInMemoryDatabase("TemplateTestDb");
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
            return userDto!.Token;
        }

        [Fact]
        public async Task CreateTemplate_And_CreateBudgetFromTemplate_ShouldSucceed()
        {
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var customerResponse = await _client.PostAsJsonAsync("/api/Customers", new CreateCustomerDto
            {
                Name = "Customer T",
                Email = "cust@t.com",
                Phone = "11999999999",
                Document = "12345678901",
                Address = "Addr"
            });
            customerResponse.EnsureSuccessStatusCode();
            var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerDto>();

            var templateResponse = await _client.PostAsJsonAsync("/api/BudgetTemplates", new CreateBudgetTemplateDto
            {
                Name = "Template X",
                Description = "Desc",
                Items = new List<CreateBudgetTemplateItemDto>
                {
                    new CreateBudgetTemplateItemDto { Name = "Item 1", Description = null, Quantity = 1, UnitPrice = 49.90m }
                }
            });

            templateResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, templateResponse.StatusCode);
            var template = await templateResponse.Content.ReadFromJsonAsync<BudgetTemplateDto>();
            template!.Items[0].Description.Should().BeNull();

            var createBudgetResponse = await _client.PostAsJsonAsync($"/api/BudgetTemplates/{template.Id}/create-budget", new CreateBudgetFromTemplateDto
            {
                CustomerId = customer!.Id
            });

            createBudgetResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, createBudgetResponse.StatusCode);
            var budget = await createBudgetResponse.Content.ReadFromJsonAsync<BudgetDto>();
            budget!.Items.Should().HaveCount(1);
            budget.Items[0].Description.Should().BeNull();
        }
    }
}



