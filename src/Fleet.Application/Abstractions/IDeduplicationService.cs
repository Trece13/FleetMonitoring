namespace Fleet.Application.Abstractions;

public interface IDeduplicationService
{
    Task<bool> TryRegisterAsync(
        string vehicleId,
        double latitude,
        double longitude,
        DateTime timestamp,
        CancellationToken cancellationToken = default);
}