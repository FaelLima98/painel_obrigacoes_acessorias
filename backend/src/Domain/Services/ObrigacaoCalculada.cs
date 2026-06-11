using EAuditoria.Domain.Enums;

namespace EAuditoria.Domain.Services;

/// <summary>
/// Resultado do cálculo da <see cref="TaxObligationEngine"/> para uma obrigação
/// em uma competência específica. Imutável e sem dependência de persistência.
/// </summary>
/// <param name="Tipo">Tipo da obrigação acessória.</param>
/// <param name="Periodicidade">Mensal ou Anual.</param>
/// <param name="CompetenciaAno">Ano da competência (exercício de referência).</param>
/// <param name="CompetenciaMes">Mês da competência (1-12).</param>
/// <param name="Vencimento">Data limite de entrega, já com prorrogação de fim de semana quando aplicável.</param>
public readonly record struct ObrigacaoCalculada(
    TipoObrigacao Tipo,
    Periodicidade Periodicidade,
    int CompetenciaAno,
    int CompetenciaMes,
    DateOnly Vencimento);
