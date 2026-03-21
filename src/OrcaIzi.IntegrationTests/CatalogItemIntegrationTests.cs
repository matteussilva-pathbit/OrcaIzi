namespace OrcaIzi.IntegrationTests
{
    public class CatalogItemIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CatalogItemIntegrationTests(WebApplicationFactory<Program> factory)
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
                        options.UseInMemoryDatabase("CatalogItemTestDb");
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
        public async Task Crud_CatalogItems_ShouldSucceed()
        {
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createResponse = await _client.PostAsJsonAsync("/api/CatalogItems", new CreateCatalogItemDto
            {
                Name = "Serviço X",
                Description = "Desc",
                Category = "Serviços",
                Unit = "un",
                UnitPrice = 99.90m,
                IsActive = true
            });

            createResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<CatalogItemDto>();
            created.Should().NotBeNull();

            var listResponse = await _client.GetAsync("/api/CatalogItems?pageNumber=1&pageSize=10&onlyActive=true");
            listResponse.EnsureSuccessStatusCode();
            var list = await listResponse.Content.ReadFromJsonAsync<PagedResult<CatalogItemDto>>();
            list!.Items.Should().ContainSingle(x => x.Id == created!.Id);

            var updateResponse = await _client.PutAsJsonAsync($"/api/CatalogItems/{created!.Id}", new CreateCatalogItemDto
            {
                Name = "Serviço Y",
                Description = "Nova",
                Category = "Serviços",
                Unit = "un",
                UnitPrice = 120m,
                IsActive = true
            });
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            var getResponse = await _client.GetAsync($"/api/CatalogItems/{created.Id}");
            getResponse.EnsureSuccessStatusCode();
            var updated = await getResponse.Content.ReadFromJsonAsync<CatalogItemDto>();
            updated!.Name.Should().Be("Serviço Y");
            updated.UnitPrice.Should().Be(120m);

            var deleteResponse = await _client.DeleteAsync($"/api/CatalogItems/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }
    }
}



