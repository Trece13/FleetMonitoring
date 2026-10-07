namespace Fleet.Application.Abstractions;

public interface IVehicleCommands
{
    Task<bool> DeleteAsync(
        string vehicleId,
        CancellationToken cancellationToken = default
    );
}