namespace EAuditoria.Domain.Enums;

/// <summary>
/// Status de uma obrigação para uma empresa em uma competência específica.
/// Calculado cruzando o resultado da engine com as entregas registradas.
/// </summary>
public enum StatusObrigacao
{
    /// <summary>Vencimento futuro, sem entrega registrada.</summary>
    Pendente = 1,

    /// <summary>Vencimento passado, sem entrega registrada.</summary>
    Atrasada = 2,

    /// <summary>Existe registro na tabela Entregas.</summary>
    Entregue = 3,

    /// <summary>Obrigação não se aplica ao regime da empresa.</summary>
    NaoAplicavel = 4
}
