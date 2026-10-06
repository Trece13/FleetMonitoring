namespace Fleet.Domain.Models;

public sealed record PositionUpdateResult(
    bool IsStale,
    bool StopAlertShouldBeCreated);