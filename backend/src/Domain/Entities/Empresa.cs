using EAuditoria.Domain.Enums;

namespace EAuditoria.Domain.Entities;

/// <summary>
/// Empresa (CNPJ) controlada pelo escritório contábil.
/// O regime determina quais obrigações se aplicam (via TaxObligationEngine).
/// </summary>
public class Empresa
{
    public Guid Id { get; set; }
    public string NomeFantasia { get; set; } = string.Empty;

    /// <summary>CNPJ apenas com dígitos (14 caracteres).</summary>
    public string Cnpj { get; set; } = string.Empty;

    public RegimeTributario Regime { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();
}
