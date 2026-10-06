using Fleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fleet.Infrastructure.SqlServer.Configurations;

public sealed class AlertConfiguration
    : IEntityTypeConfiguration<Alert>
{
    public void Configure(
        EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alerts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(x =>
            new
            {
                x.VehicleId,
                x.CreatedAtUtc
            });

        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(x => x.VehicleId);
    }
}