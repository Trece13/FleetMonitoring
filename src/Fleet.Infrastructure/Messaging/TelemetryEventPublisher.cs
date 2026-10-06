using Fleet.Application.Abstractions;
using Fleet.Contracts.Telemetry;
using MassTransit;

namespace Fleet.Infrastructure.Messaging;

public sealed class TelemetryEventPublisher
    : ITelemetryEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public TelemetryEventPublisher(
        IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync( 
        TelemetryReceived telemetry,
        CancellationToken cancellationToken = default)
    {
        return _publishEndpoint.Publish(
            telemetry,
            cancellationToken);
    }
}