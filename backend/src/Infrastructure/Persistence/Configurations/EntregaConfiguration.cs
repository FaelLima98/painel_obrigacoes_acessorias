using EAuditoria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EAuditoria.Infrastructure.Persistence.Configurations;

public class EntregaConfiguration : IEntityTypeConfiguration<Entrega>
{
    public void Configure(EntityTypeBuilder<Entrega> builder)
    {
        builder.ToTable("Entregas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TipoObrigacao)
            .HasConversion<int>();

        // Impede entrega duplicada para a mesma empresa/obrigação/competência
        builder.HasIndex(e => new
        {
            e.EmpresaId,
            e.TipoObrigacao,
            e.CompetenciaAno,
            e.CompetenciaMes
        }).IsUnique();
    }
}
