namespace Fleet.Application.Models;

public sealed record LatestVehicleState(
    string VehicleId,
    double Latitude,
    double Longitude,
    string Status,
    DateTime LastSeenAtUtc);