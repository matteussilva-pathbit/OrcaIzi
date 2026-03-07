using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrcaIzi.Application.Interfaces;
using OrcaIzi.Application.Services;
using System.Reflection;

namespace OrcaIzi.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IBudgetAppService, BudgetAppService>();
            services.AddScoped<ICustomerAppService, CustomerAppService>();

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
