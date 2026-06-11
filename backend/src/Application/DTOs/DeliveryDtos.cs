using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs;

public record CreateDeliveryRequest(
    Guid EmpresaId,
    TipoObrigacao TipoObrigacao,
    int CompetenciaAno,
    int CompetenciaMes,
    DateOnly DataEntrega);

public record DeliveryResponse(
    Guid Id,
    Guid EmpresaId,
    TipoObrigacao TipoObrigacao,
    int CompetenciaAno,
    int CompetenciaMes,
    DateOnly DataEntrega,
    DateTime CreatedAt);
