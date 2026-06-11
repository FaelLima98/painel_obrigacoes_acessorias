using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EAuditoria.Infrastructure.Persistence;

/// <summary>
/// Aplica migrations pendentes na inicialização e popula o banco com empresas de
/// demonstração caso esteja vazio (8 empresas: 3 Simples, 3 L.Presumido, 2 L.Real).
/// </summary>
public class DatabaseSeeder(
    IServiceProvider serviceProvider,
    ILogger<DatabaseSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        logger.LogInformation("Aplicando migrations...");
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Empresas.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Banco já populado — seed ignorado.");
            return;
        }

        logger.LogInformation("Banco vazio — inserindo empresas de demonstração.");
        db.Empresas.AddRange(BuildEmpresasDemo());
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seed concluído.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static IEnumerable<Empresa> BuildEmpresasDemo()
    {
        var now = DateTime.UtcNow;

        (string Nome, string Cnpj, RegimeTributario Regime)[] dados =
        [
            // 3 Simples Nacional
            ("Padaria Pão Quente",        "12345678000101", RegimeTributario.SimplesNacional),
            ("Mercadinho da Esquina",     "12345678000102", RegimeTributario.SimplesNacional),
            ("Studio Beleza & Cia",       "12345678000103", RegimeTributario.SimplesNacional),

            // 3 Lucro Presumido
            ("Construtora Horizonte Ltda","23456789000110", RegimeTributario.LucroPresumido),
            ("TransLog Transportes",      "23456789000111", RegimeTributario.LucroPresumido),
            ("Clínica Saúde Integral",    "23456789000112", RegimeTributario.LucroPresumido),

            // 2 Lucro Real
            ("Indústria MetalForte S.A.", "34567890000120", RegimeTributario.LucroReal),
            ("AgroNorte Comércio S.A.",   "34567890000121", RegimeTributario.LucroReal),

            // 2 Isentas
            ("Arcellor Mittal", "06976063000164", RegimeTributario.ImunidadeIsencao),
            ("Riachuelo",   "16436333000106", RegimeTributario.ImunidadeIsencao),
        ];

        return dados.Select(d => new Empresa
        {
            Id = Guid.NewGuid(),
            NomeFantasia = d.Nome,
            Cnpj = d.Cnpj,
            Regime = d.Regime,
            CreatedAt = now
        });
    }
}
