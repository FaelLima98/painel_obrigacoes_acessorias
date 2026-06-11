using EAuditoria.Domain.Entities;

namespace EAuditoria.Application.Abstractions;

public interface IEmpresaRepository
{
    Task<(IReadOnlyList<Empresa> Items, int Total)> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct);

    Task<IReadOnlyList<Empresa>> GetAllAsync(CancellationToken ct);
    Task<Empresa?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsByCnpjAsync(string cnpj, CancellationToken ct);
    Task<int> CountAsync(CancellationToken ct);
    Task AddAsync(Empresa empresa, CancellationToken ct);
    Task DeleteAsync(Empresa empresa, CancellationToken ct);
}
