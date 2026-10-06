using Fleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fleet.Infrastructure.SqlServer.Configurations;

public sealed class VehicleStateConfiguration
    : IEntityTypeConfiguration<VehicleState>
{
    public void Configure(
        EntityTypeBuilder<VehicleState> builder)
    {
        builder.ToTable("VehicleStates");

        builder.HasKey(x => x.VehicleId);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne<Vehicle>()
            .WithOne()
            .HasForeignKey<VehicleState>(
                x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}