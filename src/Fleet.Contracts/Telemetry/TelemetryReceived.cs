namespace Fleet.Contracts.Telemetry;

public sealed record TelemetryReceived(
    Guid EventId,
    string VehicleId,
    double Latitude,
    double Longitude,
    DateTime RecordedAtUtc,
    DateTime ReceivedAtUtc
);