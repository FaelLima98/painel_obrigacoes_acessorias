using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs;

public record CompanyResponse(
    Guid Id,
    string NomeFantasia,
    string Cnpj,
    RegimeTributario Regime,
    DateTime CreatedAt);

public record CreateCompanyRequest(
    string NomeFantasia,
    string Cnpj,
    RegimeTributario Regime);
