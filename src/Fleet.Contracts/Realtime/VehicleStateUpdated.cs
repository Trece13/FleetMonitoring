namespace Fleet.Contracts.Realtime;

public sealed record VehicleStateUpdated(
    string VehicleId,
    double Latitude,
    double Longitude,
    string Status,
    DateTime LastSeenAtUtc);