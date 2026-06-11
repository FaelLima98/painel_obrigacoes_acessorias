using EAuditoria.Domain.Enums;
using EAuditoria.Domain.Services;
using Xunit;

namespace EAuditoria.Tests;

public class TaxObligationEngineTests
{
    private readonly TaxObligationEngine _engine = new();

    private static TipoObrigacao[] Tipos(IEnumerable<ObrigacaoCalculada> obrigacoes) =>
        obrigacoes.Select(o => o.Tipo).OrderBy(t => t).ToArray();

    // ---------------------------------------------------------------------
    // Conjunto de obrigações por regime
    // ---------------------------------------------------------------------

    [Fact]
    public void Simples_MesComum_RetornaApenasDasEEsocial()
    {
        var result = _engine.Calculate(RegimeTributario.SimplesNacional, 2025, 3);

        Assert.Equal(
            new[] { TipoObrigacao.DAS, TipoObrigacao.eSocial }.OrderBy(t => t),
            Tipos(result));
    }

    [Fact]
    public void Simples_Janeiro_IncluiAnuais()
    {
        var result = _engine.Calculate(RegimeTributario.SimplesNacional, 2024, 1);

        Assert.Equal(
            new[]
            {
                TipoObrigacao.DAS,
                TipoObrigacao.eSocial,
                TipoObrigacao.DEFIS,
                TipoObrigacao.DIRF,
                TipoObrigacao.RAIS
            }.OrderBy(t => t),
            Tipos(result));
    }

    [Fact]
    public void LucroPresumido_MesComum_RetornaMensais()
    {
        var result = _engine.Calculate(RegimeTributario.LucroPresumido, 2025, 3);

        Assert.Equal(
            new[]
            {
                TipoObrigacao.DCTF,
                TipoObrigacao.EFD_ICMS_IPI,
                TipoObrigacao.EFD_Contribuicoes,
                TipoObrigacao.EFD_Reinf,
                TipoObrigacao.eSocial
            }.OrderBy(t => t),
            Tipos(result));
    }

    [Fact]
    public void LucroPresumido_Janeiro_IncluiAnuais()
    {
        var result = _engine.Calculate(RegimeTributario.LucroPresumido, 2024, 1);

        Assert.Equal(
            new[]
            {
                TipoObrigacao.DCTF,
                TipoObrigacao.EFD_ICMS_IPI,
                TipoObrigacao.EFD_Contribuicoes,
                TipoObrigacao.EFD_Reinf,
                TipoObrigacao.eSocial,
                TipoObrigacao.SPED_ECD,
                TipoObrigacao.SPED_ECF,
                TipoObrigacao.DIRF,
                TipoObrigacao.RAIS
            }.OrderBy(t => t),
            Tipos(result));
    }

    [Fact]
    public void LucroReal_MesComum_IgualAoLucroPresumido()
    {
        var real = _engine.Calculate(RegimeTributario.LucroReal, 2025, 3);
        var presumido = _engine.Calculate(RegimeTributario.LucroPresumido, 2025, 3);

        Assert.Equal(Tipos(presumido), Tipos(real));
    }

    [Fact]
    public void LucroReal_Janeiro_IgualAoLucroPresumido()
    {
        var real = _engine.Calculate(RegimeTributario.LucroReal, 2024, 1);
        var presumido = _engine.Calculate(RegimeTributario.LucroPresumido, 2024, 1);

        Assert.Equal(Tipos(presumido), Tipos(real));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(12)]
    public void Imunidade_QualquerMes_RetornaListaVazia(int month)
    {
        var result = _engine.Calculate(RegimeTributario.ImunidadeIsencao, 2025, month);

        Assert.Empty(result);
    }

    [Fact]
    public void Anuais_NaoAparecemForaDeJaneiro()
    {
        var result = _engine.Calculate(RegimeTributario.LucroReal, 2025, 6);

        Assert.DoesNotContain(result, o => o.Periodicidade == Periodicidade.Anual);
    }

    // ---------------------------------------------------------------------
    // Vencimentos mensais
    // ---------------------------------------------------------------------

    [Fact]
    public void Das_CompetenciaJaneiro2025_Vence20Fev2025()
    {
        var das = _engine.Calculate(RegimeTributario.SimplesNacional, 2025, 1)
            .Single(o => o.Tipo == TipoObrigacao.DAS);

        // 20/02/2025 é quinta-feira → sem prorrogação
        Assert.Equal(new DateOnly(2025, 2, 20), das.Vencimento);
    }

    [Fact]
    public void Das_QuandoCaiEmDomingo_ProrrogaParaSegunda()
    {
        // Competência jun/2025 → vencimento bruto 20/07/2025 (domingo)
        var das = _engine.Calculate(RegimeTributario.SimplesNacional, 2025, 6)
            .Single(o => o.Tipo == TipoObrigacao.DAS);

        Assert.Equal(DayOfWeek.Monday, das.Vencimento.DayOfWeek);
        Assert.Equal(new DateOnly(2025, 7, 21), das.Vencimento);
    }

    [Fact]
    public void Das_QuandoCaiEmSabado_ProrrogaParaSegunda()
    {
        // Competência nov/2025 → vencimento bruto 20/12/2025 (sábado)
        var das = _engine.Calculate(RegimeTributario.SimplesNacional, 2025, 11)
            .Single(o => o.Tipo == TipoObrigacao.DAS);

        Assert.Equal(DayOfWeek.Monday, das.Vencimento.DayOfWeek);
        Assert.Equal(new DateOnly(2025, 12, 22), das.Vencimento);
    }

    [Fact]
    public void Dctf_CompetenciaMarco_Vence15Maio()
    {
        var dctf = _engine.Calculate(RegimeTributario.LucroReal, 2025, 3)
            .Single(o => o.Tipo == TipoObrigacao.DCTF);

        // Dia 15 do SEGUNDO mês seguinte (não 15/abril)
        Assert.Equal(new DateOnly(2025, 5, 15), dctf.Vencimento);
    }

    [Fact]
    public void Esocial_Vence07DoMesSeguinte()
    {
        var esocial = _engine.Calculate(RegimeTributario.SimplesNacional, 2025, 3)
            .Single(o => o.Tipo == TipoObrigacao.eSocial);

        Assert.Equal(new DateOnly(2025, 4, 7), esocial.Vencimento);
    }

    [Fact]
    public void EfdIcmsIpi_Vence15DoMesSeguinte()
    {
        var efd = _engine.Calculate(RegimeTributario.LucroReal, 2025, 3)
            .Single(o => o.Tipo == TipoObrigacao.EFD_ICMS_IPI);

        Assert.Equal(new DateOnly(2025, 4, 15), efd.Vencimento);
    }

    // ---------------------------------------------------------------------
    // Vencimentos anuais (exercício 2024 → vence em 2025)
    // ---------------------------------------------------------------------

    [Fact]
    public void SpedEcd_Exercicio2024_Vence31Maio2025()
    {
        var sped = _engine.Calculate(RegimeTributario.LucroReal, 2024, 1)
            .Single(o => o.Tipo == TipoObrigacao.SPED_ECD);

        // 31/05/2025 cai num sábado, mas obrigações anuais NÃO são prorrogadas.
        Assert.Equal(new DateOnly(2025, 5, 31), sped.Vencimento);
    }

    [Fact]
    public void SpedEcf_Exercicio2024_Vence31Julho2025()
    {
        var sped = _engine.Calculate(RegimeTributario.LucroReal, 2024, 1)
            .Single(o => o.Tipo == TipoObrigacao.SPED_ECF);

        Assert.Equal(new DateOnly(2025, 7, 31), sped.Vencimento);
    }

    [Fact]
    public void Dirf_Exercicio2024_Vence28Fev2025()
    {
        var dirf = _engine.Calculate(RegimeTributario.SimplesNacional, 2024, 1)
            .Single(o => o.Tipo == TipoObrigacao.DIRF);

        // 2025 não é bissexto → último dia de fevereiro é 28
        Assert.Equal(new DateOnly(2025, 2, 28), dirf.Vencimento);
    }

    [Fact]
    public void Dirf_AnoSeguinteBissexto_VenceUltimoDiaCorreto()
    {
        // Exercício 2023 → vence em fev/2024 (bissexto) → 29/02/2024
        var dirf = _engine.Calculate(RegimeTributario.SimplesNacional, 2023, 1)
            .Single(o => o.Tipo == TipoObrigacao.DIRF);

        Assert.Equal(new DateOnly(2024, 2, 29), dirf.Vencimento);
    }

    [Fact]
    public void RaisEDefis_Exercicio2024_Vence31Marco2025()
    {
        var rais = _engine.Calculate(RegimeTributario.SimplesNacional, 2024, 1)
            .Single(o => o.Tipo == TipoObrigacao.RAIS);
        var defis = _engine.Calculate(RegimeTributario.SimplesNacional, 2024, 1)
            .Single(o => o.Tipo == TipoObrigacao.DEFIS);

        Assert.Equal(new DateOnly(2025, 3, 31), rais.Vencimento);
        Assert.Equal(new DateOnly(2025, 3, 31), defis.Vencimento);
    }

    // ---------------------------------------------------------------------
    // Metadados / validação
    // ---------------------------------------------------------------------

    [Fact]
    public void Periodicidade_ClassificadaCorretamente()
    {
        var jan = _engine.Calculate(RegimeTributario.LucroReal, 2024, 1).ToList();

        Assert.Equal(Periodicidade.Mensal, jan.Single(o => o.Tipo == TipoObrigacao.DCTF).Periodicidade);
        Assert.Equal(Periodicidade.Anual, jan.Single(o => o.Tipo == TipoObrigacao.SPED_ECD).Periodicidade);
    }

    [Fact]
    public void Competencia_EhPreservadaNoResultado()
    {
        var das = _engine.Calculate(RegimeTributario.SimplesNacional, 2025, 7)
            .Single(o => o.Tipo == TipoObrigacao.DAS);

        Assert.Equal(2025, das.CompetenciaAno);
        Assert.Equal(7, das.CompetenciaMes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Calculate_MesInvalido_LancaExcecao(int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _engine.Calculate(RegimeTributario.SimplesNacional, 2025, month).ToList());
    }
}
