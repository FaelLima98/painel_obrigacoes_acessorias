using EAuditoria.Domain.Enums;

namespace EAuditoria.Domain.Entities;

/// <summary>
/// Registro de entrega de uma obrigação para uma empresa em uma competência.
/// É a única informação persistida — as obrigações em si são calculadas em runtime.
/// O índice único (EmpresaId, TipoObrigacao, CompetenciaAno, CompetenciaMes)
/// impede duplicidade de entrega para a mesma competência.
/// </summary>
public class Entrega
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }

    public TipoObrigacao TipoObrigacao { get; set; }
    public int CompetenciaAno { get; set; }
    public int CompetenciaMes { get; set; }

    public DateOnly DataEntrega { get; set; }
    public DateTime CreatedAt { get; set; }

    public Empresa? Empresa { get; set; }
}
