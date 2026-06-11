using EAuditoria.Application.Abstractions;
using EAuditoria.Application.Common;
using EAuditoria.Application.DTOs;
using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.Services;

/// <summary>
/// Consolida, entre todas as empresas, as obrigações já atrasadas e as que vencem
/// nos próximos <c>diasAdiante</c> dias, ordenadas por urgência (mais antigas primeiro).
/// </summary>
public class AlertService(
    IEmpresaRepository empresas,
    IEntregaRepository entregas,
    ObligationService obligations,
    TimeProvider clock)
{
    public async Task<IReadOnlyList<AlertResponse>> GetAsync(int diasAdiante, CancellationToken ct)
    {
        diasAdiante = Math.Clamp(diasAdiante, 0, 365);

        var hoje = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        var limite = hoje.AddDays(diasAdiante);

        var todasEmpresas = await empresas.GetAllAsync(ct);
        var entregasPorEmpresa = (await entregas.GetAllAsync(ct))
            .GroupBy(e => e.EmpresaId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Janela de competências a varrer: últimos 12 meses (atrasadas recentes) e
        // ~2 meses à frente (cobre o lag de vencimento das obrigações mensais).
        var inicio = hoje.AddMonths(-12);
        var fim = hoje.AddMonths(2);

        var alerts = new List<AlertResponse>();

        foreach (var empresa in todasEmpresas)
        {
            var entregasEmpresa = entregasPorEmpresa.TryGetValue(empresa.Id, out var lista)
                ? lista
                : [];

            for (var comp = inicio; comp <= fim; comp = comp.AddMonths(1))
            {
                var resolved = obligations.Resolver(
                    empresa.Regime, comp.Year, comp.Month, entregasEmpresa, hoje);

                foreach (var o in resolved)
                {
                    // Inclui atrasadas e pendentes que vencem dentro da janela.
                    if (o.Status == StatusObrigacao.Entregue || o.Vencimento > limite)
                        continue;

                    alerts.Add(new AlertResponse(
                        empresa.Id,
                        empresa.NomeFantasia,
                        empresa.Regime,
                        o.Tipo,
                        o.Nome,
                        o.CompetenciaAno,
                        o.CompetenciaMes,
                        o.Vencimento,
                        o.Status,
                        o.Vencimento.DayNumber - hoje.DayNumber));
                }
            }
        }

        // Atrasadas (vencimento no passado) vêm naturalmente antes das próximas.
        return alerts
            .OrderBy(a => a.Vencimento)
            .ThenBy(a => a.EmpresaNome)
            .ToList();
    }
}
