using Fleet.Application.Abstractions;
using Fleet.Domain.Enums;
using Fleet.Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace Fleet.Infrastructure.Queries;

public sealed class VehicleQueries : IVehicleQueries
{
    private readonly FleetDbContext _dbContext;

    public VehicleQueries(FleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<VehicleListItem>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var data = await (
            from vehicle in _dbContext.Vehicles.AsNoTracking()

            join state in _dbContext.VehicleStates.AsNoTracking()
                on vehicle.Id equals state.VehicleId
                into states

            from state in states.DefaultIfEmpty()

            where vehicle.IsActive

            select new
            {
                vehicle.Id,
                vehicle.ExternalId,
                vehicle.Name,

                Latitude = state != null
                    ? (double?)state.Latitude
                    : null,

                Longitude = state != null
                    ? (double?)state.Longitude
                    : null,

                Status = state != null
                    ? (VehicleStatus?)state.Status
                    : null,

                LastSeenAtUtc = state != null
                    ? (DateTime?)state.LastSeenAtUtc
                    : null
            })
            .ToListAsync(cancellationToken);

        return data
            .Select(x => new VehicleListItem(
                x.Id,
                x.ExternalId,
                x.Name,
                x.Latitude,
                x.Longitude,
                x.Status?.ToString() ?? "Unknown",
                x.LastSeenAtUtc))
            .ToList();
    }

    public async Task<VehicleListItem?> GetByExternalIdAsync(
        string vehicleId,
        CancellationToken cancellationToken = default)
    {
        var data = await (
            from vehicle in _dbContext.Vehicles.AsNoTracking()

            join state in _dbContext.VehicleStates.AsNoTracking()
                on vehicle.Id equals state.VehicleId
                into states

            from state in states.DefaultIfEmpty()

            where vehicle.ExternalId == vehicleId
                  && vehicle.IsActive

            select new
            {
                vehicle.Id,
                vehicle.ExternalId,
                vehicle.Name,

                Latitude = state != null
                    ? (double?)state.Latitude
                    : null,

                Longitude = state != null
                    ? (double?)state.Longitude
                    : null,

                Status = state != null
                    ? (VehicleStatus?)state.Status
                    : null,

                LastSeenAtUtc = state != null
                    ? (DateTime?)state.LastSeenAtUtc
                    : null
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
            return null;

        return new VehicleListItem(
            data.Id,
            data.ExternalId,
            data.Name,
            data.Latitude,
            data.Longitude,
            data.Status?.ToString() ?? "Unknown",
            data.LastSeenAtUtc);
    }
}