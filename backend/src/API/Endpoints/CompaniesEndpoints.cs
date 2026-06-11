using EAuditoria.Application.DTOs;
using EAuditoria.Application.Services;

namespace EAuditoria.API.Endpoints;

public static class CompaniesEndpoints
{
    public static IEndpointRouteBuilder MapCompaniesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies").WithTags("Companies");

        group.MapGet("/", GetPaged).WithName("GetCompanies");
        group.MapPost("/", Create).WithName("CreateCompany");
        group.MapDelete("/{id:guid}", Delete).WithName("DeleteCompany");

        return app;
    }

    private static async Task<IResult> GetPaged(
        CompanyService service, CancellationToken ct,
        int page = 1, int pageSize = 20, string? search = null)
    {
        var result = await service.GetPagedAsync(page, pageSize, search, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateCompanyRequest request, CompanyService service, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return Results.Created($"/api/companies/{created.Id}", created);
    }

    private static async Task<IResult> Delete(
        Guid id, CompanyService service, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }
}
