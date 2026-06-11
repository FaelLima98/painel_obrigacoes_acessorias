using EAuditoria.Application.Abstractions;
using EAuditoria.Application.Common;
using EAuditoria.Application.DTOs;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using EAuditoria.Domain.Services;

namespace EAuditoria.Application.Services;

/// <summary>
/// Calcula as obrigações de uma empresa em uma competência e resolve o status real
/// cruzando o resultado da <see cref="TaxObligationEngine"/> com as entregas do banco.
/// </summary>
public class ObligationService(
    IEmpresaRepository empresas,
    IEntregaRepository entregas,
    TaxObligationEngine engine,
    TimeProvider clock)
{
    public async Task<IReadOnlyList<ObligationResponse>> GetByCompanyAsync(
        Guid empresaId, int year, int month, CancellationToken ct)
    {
        var empresa = await empresas.GetByIdAsync(empresaId, ct)
            ?? throw new NotFoundException("Empresa não encontrada.");

        var entregasComp = await entregas.GetByEmpresaCompetenciaAsync(empresaId, year, month, ct);
        var hoje = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

        return Resolver(empresa.Regime, year, month, entregasComp, hoje);
    }

    /// <summary>Resolve as obrigações já cruzadas com as entregas informadas.</summary>
    public IReadOnlyList<ObligationResponse> Resolver(
        RegimeTributario regime, int year, int month,
        IReadOnlyList<Entrega> entregasComp, DateOnly hoje)
    {
        var result = new List<ObligationResponse>();

        foreach (var o in engine.Calculate(regime, year, month))
        {
            var entrega = entregasComp.FirstOrDefault(e =>
                e.TipoObrigacao == o.Tipo &&
                e.CompetenciaAno == o.CompetenciaAno &&
                e.CompetenciaMes == o.CompetenciaMes);

            var status = entrega is not null
                ? StatusObrigacao.Entregue
                : o.Vencimento < hoje
                    ? StatusObrigacao.Atrasada
                    : StatusObrigacao.Pendente;

            result.Add(new ObligationResponse(
                o.Tipo,
                ObrigacaoInfo.Nome(o.Tipo),
                o.Periodicidade,
                o.CompetenciaAno,
                o.CompetenciaMes,
                o.Vencimento,
                status,
                entrega?.DataEntrega,
                entrega?.Id));
        }

        return result;
    }
}
