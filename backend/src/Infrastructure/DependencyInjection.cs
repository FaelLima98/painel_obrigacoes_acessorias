using EAuditoria.Application.Abstractions;
using EAuditoria.Infrastructure.Persistence;
using EAuditoria.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EAuditoria.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' não configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        services.AddScoped<IEntregaRepository, EntregaRepository>();

        services.AddHostedService<DatabaseSeeder>();

        return services;
    }
}
