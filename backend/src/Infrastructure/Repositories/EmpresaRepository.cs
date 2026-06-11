using EAuditoria.Application.Abstractions;
using EAuditoria.Domain.Entities;
using EAuditoria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EAuditoria.Infrastructure.Repositories;

public class EmpresaRepository(AppDbContext db) : IEmpresaRepository
{
    public async Task<(IReadOnlyList<Empresa> Items, int Total)> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct)
    {
        var query = db.Empresas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var termo = search.Trim();
            var digits = new string(termo.Where(char.IsDigit).ToArray());

            query = digits.Length > 0
                ? query.Where(e =>
                    EF.Functions.ILike(e.NomeFantasia, $"%{termo}%") || e.Cnpj.Contains(digits))
                : query.Where(e => EF.Functions.ILike(e.NomeFantasia, $"%{termo}%"));
        }

        var ordered = query.OrderBy(e => e.NomeFantasia);

        var total = await ordered.CountAsync(ct);
        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<Empresa>> GetAllAsync(CancellationToken ct) =>
        await db.Empresas.AsNoTracking().OrderBy(e => e.NomeFantasia).ToListAsync(ct);

    public async Task<Empresa?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.Empresas.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<bool> ExistsByCnpjAsync(string cnpj, CancellationToken ct) =>
        await db.Empresas.AnyAsync(e => e.Cnpj == cnpj, ct);

    public async Task<int> CountAsync(CancellationToken ct) =>
        await db.Empresas.CountAsync(ct);

    public async Task AddAsync(Empresa empresa, CancellationToken ct)
    {
        db.Empresas.Add(empresa);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Empresa empresa, CancellationToken ct)
    {
        db.Empresas.Remove(empresa);
        await db.SaveChangesAsync(ct);
    }
}
