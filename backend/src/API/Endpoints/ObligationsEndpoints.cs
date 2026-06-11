using EAuditoria.Application.Services;

namespace EAuditoria.API.Endpoints;

public static class ObligationsEndpoints
{
    public static IEndpointRouteBuilder MapObligationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/obligations").WithTags("Obligations");

        group.MapGet("/", Get).WithName("GetObligations");

        return app;
    }

    private static async Task<IResult> Get(
        Guid empresaId, int year, int month, ObligationService service, CancellationToken ct)
    {
        if (month is < 1 or > 12)
            return Results.Problem(
                title: "Parâmetro inválido.",
                detail: "O mês deve estar entre 1 e 12.",
                statusCode: StatusCodes.Status400BadRequest);

        var result = await service.GetByCompanyAsync(empresaId, year, month, ct);
        return Results.Ok(result);
    }
}
