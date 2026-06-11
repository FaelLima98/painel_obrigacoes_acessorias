using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EAuditoria.IntegrationTests;

[Collection("api")]
public class CompaniesApiTests(IntegrationTestFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_Retorna200EHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", body);
    }

    [Fact]
    public async Task GetCompanies_RetornaSeedAutomatico()
    {
        var page = await (await _client.GetAsync("/api/companies?pageSize=50")).ReadAsync<Paged<CompanyDto>>();

        Assert.NotNull(page);
        // O seed insere 10 empresas de demonstração.
        Assert.True(page!.Total >= 10);
        Assert.Contains(page.Items, c => c.NomeFantasia == "Padaria Pão Quente");
    }

    [Fact]
    public async Task CreateCompany_Valida_Retorna201_ERecuperavelPorBusca()
    {
        var cnpj = CnpjGen.Random();
        var create = await _client.PostAsJsonAsync("/api/companies", new
        {
            nomeFantasia = "Empresa Integração Alpha",
            cnpj,
            regime = 2,
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.ReadAsync<CompanyDto>();
        Assert.NotNull(created);
        Assert.Equal(cnpj, created!.Cnpj); // armazenado só com dígitos

        var found = await (await _client.GetAsync("/api/companies?search=Integração Alpha"))
            .ReadAsync<Paged<CompanyDto>>();
        Assert.Contains(found!.Items, c => c.Id == created.Id);
    }

    [Fact]
    public async Task CreateCompany_CnpjInvalido_Retorna400ComErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/companies", new
        {
            nomeFantasia = "",
            cnpj = "123",
            regime = 99,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadAsync<ProblemDto>();
        Assert.NotNull(problem!.Errors);
        Assert.True(problem.Errors!.ContainsKey("Cnpj"));
        Assert.True(problem.Errors.ContainsKey("NomeFantasia"));
    }

    [Fact]
    public async Task CreateCompany_CnpjDuplicado_Retorna409()
    {
        var cnpj = CnpjGen.Random();
        var payload = new { nomeFantasia = "Empresa Beta", cnpj, regime = 1 };

        var first = await _client.PostAsJsonAsync("/api/companies", payload);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/companies", payload);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task DeleteCompany_Inexistente_Retorna404()
    {
        var response = await _client.DeleteAsync($"/api/companies/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
