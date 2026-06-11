using EAuditoria.Application.Services;

namespace EAuditoria.API.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard");

        group.MapGet("/", Get).WithName("GetDashboard");

        return app;
    }

    private static async Task<IResult> Get(
        DashboardService service, CancellationToken ct, int? year = null, int? month = null)
    {
        var now = DateTime.UtcNow;
        var result = await service.GetAsync(year ?? now.Year, month ?? now.Month, ct);
        return Results.Ok(result);
    }
}
