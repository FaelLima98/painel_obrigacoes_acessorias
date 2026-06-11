using EAuditoria.Application.Abstractions;
using EAuditoria.Application.Common;
using EAuditoria.Application.DTOs;
using EAuditoria.Domain.Entities;

namespace EAuditoria.Application.Services;

public class DeliveryService(
    IEmpresaRepository empresas,
    IEntregaRepository entregas,
    TimeProvider clock)
{
    public async Task<DeliveryResponse> RegisterAsync(CreateDeliveryRequest request, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.CompetenciaMes is < 1 or > 12)
            errors[nameof(request.CompetenciaMes)] = ["O mês da competência deve estar entre 1 e 12."];

        if (request.CompetenciaAno is < 2000 or > 2100)
            errors[nameof(request.CompetenciaAno)] = ["Ano de competência inválido."];

        if (!Enum.IsDefined(request.TipoObrigacao))
            errors[nameof(request.TipoObrigacao)] = ["Tipo de obrigação inválido."];

        if (errors.Count > 0)
            throw new ValidationException(errors);

        _ = await empresas.GetByIdAsync(request.EmpresaId, ct)
            ?? throw new NotFoundException("Empresa não encontrada.");

        if (await entregas.ExistsAsync(
                request.EmpresaId, request.TipoObrigacao,
                request.CompetenciaAno, request.CompetenciaMes, ct))
        {
            throw new ConflictException(
                "Já existe uma entrega registrada para esta obrigação nesta competência.");
        }

        var entrega = new Entrega
        {
            Id = Guid.NewGuid(),
            EmpresaId = request.EmpresaId,
            TipoObrigacao = request.TipoObrigacao,
            CompetenciaAno = request.CompetenciaAno,
            CompetenciaMes = request.CompetenciaMes,
            DataEntrega = request.DataEntrega,
            CreatedAt = clock.GetUtcNow().UtcDateTime
        };

        await entregas.AddAsync(entrega, ct);

        return new DeliveryResponse(
            entrega.Id, entrega.EmpresaId, entrega.TipoObrigacao,
            entrega.CompetenciaAno, entrega.CompetenciaMes,
            entrega.DataEntrega, entrega.CreatedAt);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entrega = await entregas.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Entrega não encontrada.");

        await entregas.DeleteAsync(entrega, ct);
    }
}
