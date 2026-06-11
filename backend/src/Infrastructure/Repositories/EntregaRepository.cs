using EAuditoria.Application.Abstractions;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using EAuditoria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EAuditoria.Infrastructure.Repositories;

public class EntregaRepository(AppDbContext db) : IEntregaRepository
{
    public async Task<IReadOnlyList<Entrega>> GetByEmpresaCompetenciaAsync(
        Guid empresaId, int ano, int mes, CancellationToken ct) =>
        await db.Entregas.AsNoTracking()
            .Where(e => e.EmpresaId == empresaId
                     && e.CompetenciaAno == ano
                     && e.CompetenciaMes == mes)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Entrega>> GetAllAsync(CancellationToken ct) =>
        await db.Entregas.AsNoTracking().ToListAsync(ct);

    public async Task<Entrega?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.Entregas.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<bool> ExistsAsync(
        Guid empresaId, TipoObrigacao tipo, int ano, int mes, CancellationToken ct) =>
        await db.Entregas.AnyAsync(e =>
            e.EmpresaId == empresaId
            && e.TipoObrigacao == tipo
            && e.CompetenciaAno == ano
            && e.CompetenciaMes == mes, ct);

    public async Task AddAsync(Entrega entrega, CancellationToken ct)
    {
        db.Entregas.Add(entrega);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Entrega entrega, CancellationToken ct)
    {
        db.Entregas.Remove(entrega);
        await db.SaveChangesAsync(ct);
    }
}
