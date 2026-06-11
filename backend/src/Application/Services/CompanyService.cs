using EAuditoria.Application.Abstractions;
using EAuditoria.Application.Common;
using EAuditoria.Application.DTOs;
using EAuditoria.Domain.Entities;

namespace EAuditoria.Application.Services;

public class CompanyService(IEmpresaRepository empresas, TimeProvider clock)
{
    public async Task<PagedResult<CompanyResponse>> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await empresas.GetPagedAsync(page, pageSize, search, ct);

        return new PagedResult<CompanyResponse>(
            [.. items.Select(Map)], page, pageSize, total);
    }

    public async Task<CompanyResponse> CreateAsync(CreateCompanyRequest request, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.NomeFantasia))
            errors[nameof(request.NomeFantasia)] = ["O nome fantasia é obrigatório."];

        var cnpj = Cnpj.Normalizar(request.Cnpj);
        if (!Cnpj.EhValido(cnpj))
            errors[nameof(request.Cnpj)] = ["CNPJ inválido."];

        if (!Enum.IsDefined(request.Regime))
            errors[nameof(request.Regime)] = ["Regime tributário inválido."];

        if (errors.Count > 0)
            throw new ValidationException(errors);

        if (await empresas.ExistsByCnpjAsync(cnpj, ct))
            throw new ConflictException("Já existe uma empresa com este CNPJ.");

        var empresa = new Empresa
        {
            Id = Guid.NewGuid(),
            NomeFantasia = request.NomeFantasia.Trim(),
            Cnpj = cnpj,
            Regime = request.Regime,
            CreatedAt = clock.GetUtcNow().UtcDateTime
        };

        await empresas.AddAsync(empresa, ct);
        return Map(empresa);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var empresa = await empresas.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Empresa não encontrada.");

        await empresas.DeleteAsync(empresa, ct);
    }

    private static CompanyResponse Map(Empresa e) =>
        new(e.Id, e.NomeFantasia, e.Cnpj, e.Regime, e.CreatedAt);
}
