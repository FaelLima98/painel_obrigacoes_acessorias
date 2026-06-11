using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs;

public record AlertResponse(
    Guid EmpresaId,
    string EmpresaNome,
    RegimeTributario Regime,
    TipoObrigacao Tipo,
    string Nome,
    int CompetenciaAno,
    int CompetenciaMes,
    DateOnly Vencimento,
    StatusObrigacao Status,
    int DiasRestantes);
