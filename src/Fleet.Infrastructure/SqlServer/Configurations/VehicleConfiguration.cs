using Fleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fleet.Infrastructure.SqlServer.Configurations;

public sealed class VehicleConfiguration
    : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(
        EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExternalId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.ExternalId)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();
    }
}