namespace EAuditoria.Domain.Enums;

/// <summary>
/// Regime tributário da empresa. Determina quais obrigações acessórias se aplicam.
/// </summary>
public enum RegimeTributario
{
    SimplesNacional = 1,
    LucroPresumido = 2,
    LucroReal = 3,
    ImunidadeIsencao = 4
}
