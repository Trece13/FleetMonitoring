namespace Fleet.Application.Abstractions;

public interface IVehicleQueries
{
    Task<IReadOnlyList<VehicleListItem>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<VehicleListItem?> GetByExternalIdAsync(
        string vehicleId,
        CancellationToken cancellationToken = default);
}

public sealed record VehicleListItem(
    Guid Id,
    string VehicleId,
    string Name,
    double? Latitude,
    double? Longitude,
    string Status,
    DateTime? LastSeenAtUtc);