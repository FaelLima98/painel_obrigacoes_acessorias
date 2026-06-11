using EAuditoria.Application.Abstractions;
using EAuditoria.Application.DTOs;
using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.Services;

/// <summary>
/// Agrega KPIs de uma competência (ano/mês) entre todas as empresas.
/// </summary>
public class DashboardService(
    IEmpresaRepository empresas,
    IEntregaRepository entregas,
    ObligationService obligations,
    TimeProvider clock)
{
    public async Task<DashboardResponse> GetAsync(int year, int month, CancellationToken ct)
    {
        if (month is < 1 or > 12)
            month = clock.GetUtcNow().Month;

        var hoje = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

        var todasEmpresas = await empresas.GetAllAsync(ct);
        var entregasPorEmpresa = (await entregas.GetAllAsync(ct))
            .GroupBy(e => e.EmpresaId)
            .ToDictionary(g => g.Key, g => g.ToList());

        int obrigacoes = 0, pendentes = 0, entregues = 0, atrasadas = 0;

        foreach (var empresa in todasEmpresas)
        {
            var entregasEmpresa = entregasPorEmpresa.TryGetValue(empresa.Id, out var lista)
                ? lista
                : [];

            foreach (var o in obligations.Resolver(empresa.Regime, year, month, entregasEmpresa, hoje))
            {
                obrigacoes++;
                switch (o.Status)
                {
                    case StatusObrigacao.Pendente: pendentes++; break;
                    case StatusObrigacao.Entregue: entregues++; break;
                    case StatusObrigacao.Atrasada: atrasadas++; break;
                }
            }
        }

        return new DashboardResponse(
            todasEmpresas.Count, obrigacoes, pendentes, entregues, atrasadas);
    }
}
