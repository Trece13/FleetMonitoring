using Fleet.Application.Abstractions;
using Fleet.Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace Fleet.Infrastructure.Queries;

public sealed class AlertQueries : IAlertQueries
{
    private readonly FleetDbContext _dbContext;

    public AlertQueries(FleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AlertListItem>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var data = await (
            from alert in _dbContext.Alerts.AsNoTracking()
            join vehicle in _dbContext.Vehicles.AsNoTracking()
                on alert.VehicleId equals vehicle.Id

            orderby alert.CreatedAtUtc descending

            select new
            {
                alert.Id,
                VehicleId = vehicle.ExternalId,
                alert.Type,
                alert.Message,
                alert.CreatedAtUtc,
                alert.ResolvedAtUtc
            })
            .ToListAsync(cancellationToken);

        return data
            .Select(x => new AlertListItem(
                x.Id,
                x.VehicleId,
                x.Type.ToString(),
                x.Message,
                x.CreatedAtUtc,
                x.ResolvedAtUtc))
            .ToList();
    }
}