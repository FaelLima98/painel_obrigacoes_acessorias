using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EAuditoria.IntegrationTests;

[Collection("api")]
public class CalendarApiTests(IntegrationTestFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private const int Das = 1;
    private const int Esocial = 9;

    private async Task<Guid> CriarEmpresaSimplesAsync(string nomeSuffix)
    {
        var response = await _client.PostAsJsonAsync("/api/companies", new
        {
            nomeFantasia = $"Calendário {nomeSuffix}",
            cnpj = CnpjGen.Random(),
            regime = 1, // Simples Nacional
        });
        response.EnsureSuccessStatusCode();
        var created = await response.ReadAsync<CompanyDto>();
        return created!.Id;
    }

    [Fact]
    public async Task Obligations_SimplesMesComum_RetornaDasEEsocial()
    {
        var empresaId = await CriarEmpresaSimplesAsync("220000000001");

        var obrigacoes = await (await _client.GetAsync(
            $"/api/obligations?empresaId={empresaId}&year=2025&month=3"))
            .ReadAsync<List<ObligationDto>>();

        Assert.NotNull(obrigacoes);
        var tipos = obrigacoes!.Select(o => o.Tipo).OrderBy(t => t).ToArray();
        Assert.Equal([Das, Esocial], tipos);
    }

    [Fact]
    public async Task Obligations_EmpresaInexistente_Retorna404()
    {
        var response = await _client.GetAsync(
            $"/api/obligations?empresaId={Guid.NewGuid()}&year=2025&month=3");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeliveryFlow_Marca_Status_Conflito_Desfaz()
    {
        var empresaId = await CriarEmpresaSimplesAsync("220000000002");

        // 1. Registra a entrega do DAS competência 03/2025
        var createDelivery = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            empresaId,
            tipoObrigacao = Das,
            competenciaAno = 2025,
            competenciaMes = 3,
            dataEntrega = "2025-04-18",
        });
        Assert.Equal(HttpStatusCode.Created, createDelivery.StatusCode);
        var delivery = await createDelivery.ReadAsync<DeliveryDto>();

        // 2. A obrigação agora aparece como Entregue (status 3) com o entregaId
        var obrigacoes = await (await _client.GetAsync(
            $"/api/obligations?empresaId={empresaId}&year=2025&month=3"))
            .ReadAsync<List<ObligationDto>>();
        var das = obrigacoes!.Single(o => o.Tipo == Das);
        Assert.Equal(3, das.Status);
        Assert.Equal(delivery!.Id, das.EntregaId);

        // 3. Entrega duplicada para a mesma competência → 409
        var duplicate = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            empresaId,
            tipoObrigacao = Das,
            competenciaAno = 2025,
            competenciaMes = 3,
            dataEntrega = "2025-04-18",
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        // 4. Desfaz a entrega → 204; obrigação deixa de ser Entregue
        var delete = await _client.DeleteAsync($"/api/deliveries/{delivery.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var depois = await (await _client.GetAsync(
            $"/api/obligations?empresaId={empresaId}&year=2025&month=3"))
            .ReadAsync<List<ObligationDto>>();
        Assert.NotEqual(3, depois!.Single(o => o.Tipo == Das).Status);
    }

    [Fact]
    public async Task Dashboard_RetornaContagensConsistentes()
    {
        var dashboard = await (await _client.GetAsync("/api/dashboard?year=2025&month=3"))
            .ReadAsync<DashboardDto>();

        Assert.NotNull(dashboard);
        Assert.True(dashboard!.TotalEmpresas >= 10);
        // A soma dos status bate com o total de obrigações do mês.
        Assert.Equal(
            dashboard.ObrigacoesMes,
            dashboard.Pendentes + dashboard.Entregues + dashboard.Atrasadas);
    }

    [Fact]
    public async Task Alerts_RetornaListaOrdenadaPorVencimento()
    {
        var alerts = await (await _client.GetAsync("/api/alerts?diasAdiante=30"))
            .ReadAsync<List<AlertDto>>();

        Assert.NotNull(alerts);
        var vencimentos = alerts!.Select(a => a.Vencimento).ToList();
        Assert.Equal(vencimentos.OrderBy(v => v), vencimentos); // ordem ascendente
    }
}
