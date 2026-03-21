﻿namespace OrcaIzi.IntegrationTests
{
    public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
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

                    // Add InMemory DbContext
                    services.AddDbContext<OrcaIziDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                        options.UseInternalServiceProvider(inMemoryProvider);
                    });
                });
            });
        }

        [Fact]
        public async Task Register_ShouldSucceed_WhenDataIsValid()
        {
            // Arrange
            var client = _factory.CreateClient();
            var registerDto = new RegisterDto
            {
                Username = "integrationtestuser",
                Email = "integration@test.com",
                Password = "Password123!",
                ZipCode = "12345678",
                Street = "Integration St",
                Number = "100",
                Neighborhood = "Test Neighborhood",
                City = "Test City",
                State = "TS"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Status Code: {response.StatusCode}, Content: {content}");
            }
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(userDto);
            Assert.Equal(registerDto.Email, userDto.Email);
        }
    }
}



