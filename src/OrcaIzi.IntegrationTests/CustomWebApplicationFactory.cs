using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrcaIzi.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrcaIzi.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Singleton service provider for In-Memory DB to ensure persistence across requests
        private readonly IServiceProvider _inMemoryServiceProvider;

        public CustomWebApplicationFactory()
        {
            _inMemoryServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
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
                // Remove the existing DbContext registration
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<OrcaIziDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                var optionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions));

                if (optionsDescriptor != null)
                {
                    services.Remove(optionsDescriptor);
                }

                var dbConnectionDescriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(System.Data.Common.DbConnection));

                if (dbConnectionDescriptor != null)
                {
                    services.Remove(dbConnectionDescriptor);
                }

                var sqlServerAssembly = typeof(SqlServerDbContextOptionsExtensions).Assembly;
                var sqlServerDescriptors = services
                    .Where(d => (d.ImplementationType?.Assembly == sqlServerAssembly) || d.ServiceType.Assembly == sqlServerAssembly)
                    .ToList();
                foreach (var d in sqlServerDescriptors)
                {
                    services.Remove(d);
                }

                // Add DbContext using an in-memory database for testing
                // We use a shared internal service provider to ensure the InMemory database persists
                // across different contexts resolved within the same test session.
                services.AddDbContext<OrcaIziDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.UseInternalServiceProvider(_inMemoryServiceProvider);
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context (OrcaIziDbContext)
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<OrcaIziDbContext>();

                    // Ensure the database is created.
                    db.Database.EnsureCreated();
                }
            });
        }
    }
}
