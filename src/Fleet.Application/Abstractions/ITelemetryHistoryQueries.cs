namespace Fleet.Application.Abstractions;

public interface ITelemetryHistoryQueries
{
    Task<IReadOnlyList<TelemetryHistoryItem>> GetByVehicleAsync(
        string vehicleId,
        int limit = 100,
        CancellationToken cancellationToken = default);
}

public sealed record TelemetryHistoryItem(
    Guid EventId,
    double Latitude,
    double Longitude,
    DateTime RecordedAtUtc);