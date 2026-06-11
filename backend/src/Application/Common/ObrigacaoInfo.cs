using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.Common;

/// <summary>Nomes legíveis das obrigações para exibição na UI.</summary>
public static class ObrigacaoInfo
{
    public static string Nome(TipoObrigacao tipo) => tipo switch
    {
        TipoObrigacao.DAS => "DAS",
        TipoObrigacao.DEFIS => "DEFIS",
        TipoObrigacao.DCTF => "DCTF",
        TipoObrigacao.EFD_ICMS_IPI => "EFD-ICMS/IPI",
        TipoObrigacao.EFD_Contribuicoes => "EFD Contribuições",
        TipoObrigacao.EFD_Reinf => "EFD-Reinf",
        TipoObrigacao.SPED_ECD => "SPED ECD",
        TipoObrigacao.SPED_ECF => "SPED ECF",
        TipoObrigacao.eSocial => "eSocial",
        TipoObrigacao.DIRF => "DIRF",
        TipoObrigacao.RAIS => "RAIS",
        _ => tipo.ToString()
    };
}
