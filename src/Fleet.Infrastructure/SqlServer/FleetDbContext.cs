using Fleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fleet.Infrastructure.SqlServer;

public sealed class FleetDbContext
    : DbContext
{
    public FleetDbContext(
        DbContextOptions<FleetDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles =>
        Set<Vehicle>();

    public DbSet<VehicleState> VehicleStates =>
        Set<VehicleState>();

    public DbSet<Alert> Alerts =>
        Set<Alert>();

    public DbSet<ProcessedMessage> ProcessedMessages =>
        Set<ProcessedMessage>();

    public DbSet<OutboxMessage> OutboxMessages =>
    Set<OutboxMessage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FleetDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}