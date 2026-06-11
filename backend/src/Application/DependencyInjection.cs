using EAuditoria.Application.Services;
using EAuditoria.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EAuditoria.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<TaxObligationEngine>();

        services.AddScoped<CompanyService>();
        services.AddScoped<ObligationService>();
        services.AddScoped<DeliveryService>();
        services.AddScoped<AlertService>();
        services.AddScoped<DashboardService>();

        return services;
    }
}
