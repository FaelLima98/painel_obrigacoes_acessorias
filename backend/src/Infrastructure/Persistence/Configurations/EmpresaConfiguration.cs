using EAuditoria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EAuditoria.Infrastructure.Persistence.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NomeFantasia)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Cnpj)
            .IsRequired()
            .HasMaxLength(14)
            .IsFixedLength();

        builder.Property(e => e.Regime)
            .HasConversion<int>();

        // CNPJ único entre empresas
        builder.HasIndex(e => e.Cnpj).IsUnique();

        builder.HasMany(e => e.Entregas)
            .WithOne(e => e.Empresa)
            .HasForeignKey(e => e.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
