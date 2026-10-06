namespace Fleet.Application.Abstractions;

public interface IAlertQueries
{
    Task<IReadOnlyList<AlertListItem>> GetAllAsync(
        CancellationToken cancellationToken = default);
}

public sealed record AlertListItem(
    Guid Id,
    string VehicleId,
    string Type,
    string Message,
    DateTime CreatedAtUtc,
    DateTime? ResolvedAtUtc);