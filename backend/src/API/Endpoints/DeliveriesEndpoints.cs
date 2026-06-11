using EAuditoria.Application.DTOs;
using EAuditoria.Application.Services;

namespace EAuditoria.API.Endpoints;

public static class DeliveriesEndpoints
{
    public static IEndpointRouteBuilder MapDeliveriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/deliveries").WithTags("Deliveries");

        group.MapPost("/", Register).WithName("RegisterDelivery");
        group.MapDelete("/{id:guid}", Delete).WithName("DeleteDelivery");

        return app;
    }

    private static async Task<IResult> Register(
        CreateDeliveryRequest request, DeliveryService service, CancellationToken ct)
    {
        var created = await service.RegisterAsync(request, ct);
        return Results.Created($"/api/deliveries/{created.Id}", created);
    }

    private static async Task<IResult> Delete(
        Guid id, DeliveryService service, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }
}
