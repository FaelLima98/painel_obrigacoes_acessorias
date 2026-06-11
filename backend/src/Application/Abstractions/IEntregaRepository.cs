using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.Abstractions;

public interface IEntregaRepository
{
    Task<IReadOnlyList<Entrega>> GetByEmpresaCompetenciaAsync(
        Guid empresaId, int ano, int mes, CancellationToken ct);

    Task<IReadOnlyList<Entrega>> GetAllAsync(CancellationToken ct);
    Task<Entrega?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<bool> ExistsAsync(
        Guid empresaId, TipoObrigacao tipo, int ano, int mes, CancellationToken ct);

    Task AddAsync(Entrega entrega, CancellationToken ct);
    Task DeleteAsync(Entrega entrega, CancellationToken ct);
}
