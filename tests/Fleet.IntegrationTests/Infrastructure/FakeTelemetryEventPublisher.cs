using System.Collections.Concurrent;
using Fleet.Application.Abstractions;
using Fleet.Contracts.Telemetry;

namespace Fleet.IntegrationTests.Infrastructure;

public sealed class FakeTelemetryEventPublisher
    : ITelemetryEventPublisher
{
    private readonly ConcurrentQueue<TelemetryReceived> _published = new();

    public IReadOnlyCollection<TelemetryReceived> Published =>
        _published.ToArray();

    public Task PublishAsync(
        TelemetryReceived telemetry,
        CancellationToken cancellationToken = default)
    {
        _published.Enqueue(telemetry);

        return Task.CompletedTask;
    }

    public void Clear()
    {
        while (_published.TryDequeue(out _))
        {
        }
    }
}