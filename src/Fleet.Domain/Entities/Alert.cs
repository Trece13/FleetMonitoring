namespace Fleet.Domain.Entities;

public sealed class Alert
{
    public Guid Id { get; private set; }

    public Guid VehicleId { get; private set; }

    public string Type { get; private set; } = null!;

    public string Message { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ResolvedAtUtc { get; private set; }

    private Alert()
    {
    }

    private Alert(
        Guid vehicleId,
        string type,
        string message,
        DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();

        VehicleId = vehicleId;

        Type = type;

        Message = message;

        CreatedAtUtc = createdAtUtc;
    }

    public static Alert VehicleStopped(
        Guid vehicleId,
        DateTime createdAtUtc)
    {
        return new Alert(
            vehicleId,
            "VEHICLE_STOPPED",
            "Vehicle has remained stationary for more than one minute.",
            createdAtUtc);
    }
}