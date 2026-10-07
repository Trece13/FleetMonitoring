using System.Text.Json;
using Fleet.Application.Abstractions;
using Fleet.Contracts.Vehicles;
using Fleet.Domain.Entities;
using Fleet.Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace Fleet.Infrastructure.Commands;

public sealed class VehicleCommands
    : IVehicleCommands
{
    private readonly FleetDbContext _dbContext;

    public VehicleCommands(
        FleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> DeleteAsync(
        string vehicleId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync(
                    cancellationToken);

        var vehicle =
            await _dbContext.Vehicles
                .SingleOrDefaultAsync(
                    x =>
                        x.ExternalId == vehicleId &&
                        x.IsActive,
                    cancellationToken);

        if (vehicle is null)
        {
            return false;
        }

        // Soft delete
        vehicle.Delete();

        var deletedAtUtc =
            vehicle.DeletedAtUtc
            ?? DateTime.UtcNow;

        var integrationEvent =
            new VehicleDeleted(
                vehicle.Id,
                vehicle.ExternalId,
                deletedAtUtc
            );

        var outboxMessage =
            new OutboxMessage(
                Guid.NewGuid(),
                nameof(VehicleDeleted),
                JsonSerializer.Serialize(
                    integrationEvent
                ),
                deletedAtUtc
            );

        _dbContext.OutboxMessages.Add(
            outboxMessage
        );

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        await transaction.CommitAsync(
            cancellationToken
        );

        return true;
    }
}