using EAuditoria.Domain.Enums;

namespace EAuditoria.Domain.Services;

/// <summary>
/// Engine pura (sem I/O) que calcula as obrigações acessórias aplicáveis a um
/// regime tributário em uma competência (ano/mês).
///
/// Regras-chave:
/// - Imunidade/Isenção não tem nenhuma obrigação.
/// - eSocial, DIRF e RAIS aplicam-se a todos os regimes (exceto Imunidade).
/// - Obrigações anuais aparecem apenas na competência de janeiro (month == 1)
///   e referem-se ao exercício do <paramref name="year"/>, vencendo no ano seguinte.
/// - Obrigações mensais sofrem prorrogação para o próximo dia útil quando o
///   vencimento cai em sábado ou domingo (feriados não são considerados).
///   As datas legais fixas das obrigações anuais não são prorrogadas.
/// </summary>
public class TaxObligationEngine
{
    private static readonly TipoObrigacao[] SimplesNacional =
    [
        TipoObrigacao.DAS,
        TipoObrigacao.eSocial,
        TipoObrigacao.DEFIS,
        TipoObrigacao.DIRF,
        TipoObrigacao.RAIS
    ];

    // Lucro Presumido e Lucro Real compartilham o mesmo conjunto de obrigações.
    private static readonly TipoObrigacao[] LucroPresumidoReal =
    [
        TipoObrigacao.DCTF,
        TipoObrigacao.EFD_ICMS_IPI,
        TipoObrigacao.EFD_Contribuicoes,
        TipoObrigacao.EFD_Reinf,
        TipoObrigacao.eSocial,
        TipoObrigacao.SPED_ECD,
        TipoObrigacao.SPED_ECF,
        TipoObrigacao.DIRF,
        TipoObrigacao.RAIS
    ];

    private static readonly HashSet<TipoObrigacao> Anuais =
    [
        TipoObrigacao.DEFIS,
        TipoObrigacao.SPED_ECD,
        TipoObrigacao.SPED_ECF,
        TipoObrigacao.DIRF,
        TipoObrigacao.RAIS
    ];

    /// <summary>
    /// Calcula as obrigações aplicáveis ao <paramref name="regime"/> na competência informada.
    /// </summary>
    /// <param name="regime">Regime tributário da empresa.</param>
    /// <param name="year">Ano da competência (exercício de referência).</param>
    /// <param name="month">Mês da competência (1-12).</param>
    public IEnumerable<ObrigacaoCalculada> Calculate(RegimeTributario regime, int year, int month)
    {
        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), month, "O mês deve estar entre 1 e 12.");

        var tipos = regime switch
        {
            RegimeTributario.SimplesNacional => SimplesNacional,
            RegimeTributario.LucroPresumido => LucroPresumidoReal,
            RegimeTributario.LucroReal => LucroPresumidoReal,
            RegimeTributario.ImunidadeIsencao => [],
            _ => []
        };

        foreach (var tipo in tipos)
        {
            var periodicidade = Anuais.Contains(tipo) ? Periodicidade.Anual : Periodicidade.Mensal;

            // Obrigações anuais aparecem somente na competência de janeiro.
            if (periodicidade == Periodicidade.Anual && month != 1)
                continue;

            var vencimento = CalculateVencimento(tipo, year, month);

            // Prorrogação de fim de semana aplica-se apenas às obrigações mensais.
            if (periodicidade == Periodicidade.Mensal)
                vencimento = ProximoDiaUtil(vencimento);

            yield return new ObrigacaoCalculada(tipo, periodicidade, year, month, vencimento);
        }
    }

    private static DateOnly CalculateVencimento(TipoObrigacao tipo, int year, int month)
    {
        var competencia = new DateOnly(year, month, 1);

        return tipo switch
        {
            // Mensais
            TipoObrigacao.DAS => DiaDoMes(competencia.AddMonths(1), 20),
            TipoObrigacao.DCTF => DiaDoMes(competencia.AddMonths(2), 15),  // segundo mês seguinte
            TipoObrigacao.EFD_ICMS_IPI => DiaDoMes(competencia.AddMonths(1), 15),
            TipoObrigacao.EFD_Contribuicoes => DiaDoMes(competencia.AddMonths(1), 15),
            TipoObrigacao.EFD_Reinf => DiaDoMes(competencia.AddMonths(1), 15),
            TipoObrigacao.eSocial => DiaDoMes(competencia.AddMonths(1), 7),

            // Anuais — vencimento fixo no ano seguinte ao exercício.
            TipoObrigacao.SPED_ECD => new DateOnly(year + 1, 5, 31),
            TipoObrigacao.SPED_ECF => new DateOnly(year + 1, 7, 31),
            TipoObrigacao.DIRF => UltimoDiaDeFevereiro(year + 1),
            TipoObrigacao.RAIS => new DateOnly(year + 1, 3, 31),
            TipoObrigacao.DEFIS => new DateOnly(year + 1, 3, 31),

            _ => throw new ArgumentOutOfRangeException(
                nameof(tipo), tipo, "Tipo de obrigação sem regra de vencimento.")
        };
    }

    private static DateOnly DiaDoMes(DateOnly mes, int dia) => new(mes.Year, mes.Month, dia);

    private static DateOnly UltimoDiaDeFevereiro(int year) => new(year, 2, DateTime.DaysInMonth(year, 2));

    private static DateOnly ProximoDiaUtil(DateOnly date)
    {
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            date = date.AddDays(1);
        return date;
    }
}
