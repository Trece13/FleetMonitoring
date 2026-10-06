namespace Fleet.Contracts.Telemetry;

public sealed record ReceiveTelemetryRequest(
    string VehicleId,
    double Latitude,
    double Longitude,
    DateTime Timestamp
);