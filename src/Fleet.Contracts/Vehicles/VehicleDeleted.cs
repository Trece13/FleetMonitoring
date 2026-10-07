namespace Fleet.Contracts.Vehicles;

public sealed record VehicleDeleted(
    Guid VehicleId,
    string ExternalId,
    DateTime DeletedAtUtc
);