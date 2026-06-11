using EAuditoria.Application.Services;

namespace EAuditoria.API.Endpoints;

public static class AlertsEndpoints
{
    public static IEndpointRouteBuilder MapAlertsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/alerts").WithTags("Alerts");

        group.MapGet("/", Get).WithName("GetAlerts");

        return app;
    }

    private static async Task<IResult> Get(
        AlertService service, CancellationToken ct, int diasAdiante = 30)
    {
        var result = await service.GetAsync(diasAdiante, ct);
        return Results.Ok(result);
    }
}
