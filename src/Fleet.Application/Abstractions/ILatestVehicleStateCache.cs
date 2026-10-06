using Fleet.Application.Models;

namespace Fleet.Application.Abstractions;

public interface ILatestVehicleStateCache
{
    Task SetAsync(
        LatestVehicleState state,
        CancellationToken cancellationToken = default);

    Task<LatestVehicleState?> GetAsync(
        string vehicleId,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string vehicleId,
        CancellationToken cancellationToken = default);
}