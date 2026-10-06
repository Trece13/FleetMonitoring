using Fleet.Contracts.Telemetry;

namespace Fleet.Application.Abstractions;

public interface ITelemetryEventPublisher
{
    Task PublishAsync(
        TelemetryReceived telemetry,
        CancellationToken cancellationToken = default);
}