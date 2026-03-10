using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrcaIzi.Application.Interfaces.Services;
using OrcaIzi.Domain.Interfaces;
using OrcaIzi.Infrastructure.Context;
using OrcaIzi.Infrastructure.Repositories;
using OrcaIzi.Infrastructure.Services.External;
using OrcaIzi.Infrastructure.Services.Pdf;
using OrcaIzi.Infrastructure.Services;
using OrcaIzi.Infrastructure.Services.Payments;

namespace OrcaIzi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = configuration["Database:Provider"];
            if (string.Equals(provider, "InMemory", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDbContext<OrcaIziDbContext>(options =>
                    options.UseInMemoryDatabase("OrcaIzi"));
            }
            else
            {
                services.AddDbContext<OrcaIziDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null)));
            }

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IBudgetRepository, BudgetRepository>();
            services.AddScoped<IBudgetTemplateRepository, BudgetTemplateRepository>();
            services.AddScoped<OrcaIzi.Application.Interfaces.Services.IPdfService, OrcaIzi.Infrastructure.Services.Pdf.PdfService>();
            services.AddScoped<IPaymentGateway, MercadoPagoPaymentGateway>();
            services.AddScoped<IWhatsAppService, WhatsAppService>();
            services.AddScoped<ExternalApiService>();
            services.AddHttpClient("MercadoPago", client =>
            {
                client.BaseAddress = new Uri("https://api.mercadopago.com");
            });
            services.AddHttpClient();
            services.AddLogging();

            return services;
        }
    }
}
