﻿namespace OrcaIzi.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IBudgetAppService, BudgetAppService>();
            services.AddScoped<IBudgetTemplateAppService, BudgetTemplateAppService>();
            services.AddScoped<ICustomerAppService, CustomerAppService>();
            services.AddScoped<ICatalogItemAppService, CatalogItemAppService>();

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}



