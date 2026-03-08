using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrcaIzi.Application.Interfaces.Services;
using OrcaIzi.Domain.Interfaces;
using OrcaIzi.Infrastructure.Context;
using OrcaIzi.Infrastructure.Repositories;
using OrcaIzi.Infrastructure.Services.External;
using OrcaIzi.Infrastructure.Services.Pdf;

namespace OrcaIzi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrcaIziDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IBudgetRepository, BudgetRepository>();
            services.AddScoped<OrcaIzi.Application.Interfaces.Services.IPdfService, PdfService>();
            services.AddScoped<ExternalApiService>();
            services.AddHttpClient();
            services.AddLogging();

            return services;
        }
    }
}
