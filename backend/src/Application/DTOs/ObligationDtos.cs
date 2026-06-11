using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs;

public record ObligationResponse(
    TipoObrigacao Tipo,
    string Nome,
    Periodicidade Periodicidade,
    int CompetenciaAno,
    int CompetenciaMes,
    DateOnly Vencimento,
    StatusObrigacao Status,
    DateOnly? DataEntrega,
    Guid? EntregaId);
