using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace EAuditoria.IntegrationTests;

[CollectionDefinition("api")]
public class ApiCollection : ICollectionFixture<IntegrationTestFactory>;

// ---- DTOs de resposta (camelCase via case-insensitive) ----

public record CompanyDto(Guid Id, string NomeFantasia, string Cnpj, int Regime, DateTime CreatedAt);
public record Paged<T>(List<T> Items, int Page, int PageSize, int Total);

public record ObligationDto(
    int Tipo, string Nome, int Periodicidade, int CompetenciaAno, int CompetenciaMes,
    DateOnly Vencimento, int Status, DateOnly? DataEntrega, Guid? EntregaId);

public record DeliveryDto(
    Guid Id, Guid EmpresaId, int TipoObrigacao, int CompetenciaAno, int CompetenciaMes,
    DateOnly DataEntrega, DateTime CreatedAt);

public record DashboardDto(
    int TotalEmpresas, int ObrigacoesMes, int Pendentes, int Entregues, int Atrasadas);

public record AlertDto(
    Guid EmpresaId, string EmpresaNome, int Regime, int Tipo, string Nome,
    int CompetenciaAno, int CompetenciaMes, DateOnly Vencimento, int Status, int DiasRestantes);

public record ProblemDto(string? Title, int? Status, Dictionary<string, string[]>? Errors);

public static class Json
{
    public static readonly JsonSerializerOptions Options =
        new() { PropertyNameCaseInsensitive = true };

    public static Task<T?> ReadAsync<T>(this HttpResponseMessage r) =>
        r.Content.ReadFromJsonAsync<T>(Options);
}

public static class CnpjGen
{
    private static readonly int[] P1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] P2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    /// <summary>Gera um CNPJ válido a partir de 12 dígitos base.</summary>
    public static string FromBase(string baseTwelve)
    {
        var d1 = Dv(baseTwelve, P1);
        var d2 = Dv(baseTwelve + d1, P2);
        return baseTwelve + d1 + d2;
    }

    /// <summary>
    /// Gera um CNPJ válido e aleatório — evita conflito de duplicidade entre
    /// execuções (os testes não assumem um banco limpo).
    /// </summary>
    public static string Random()
    {
        var digits = new char[12];
        for (var i = 0; i < 12; i++)
            digits[i] = (char)('0' + System.Random.Shared.Next(10));
        return FromBase(new string(digits));
    }

    private static int Dv(string digits, int[] pesos)
    {
        var soma = 0;
        for (var i = 0; i < pesos.Length; i++)
            soma += (digits[i] - '0') * pesos[i];
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
