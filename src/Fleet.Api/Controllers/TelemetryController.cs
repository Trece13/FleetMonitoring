using Fleet.Application.Abstractions;
using Fleet.Contracts.Telemetry;
using Microsoft.AspNetCore.Mvc;

namespace Fleet.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public sealed class TelemetryController : ControllerBase
{
    private readonly IDeduplicationService _deduplication;
    private readonly ITelemetryEventPublisher _publisher;

    public TelemetryController(
        IDeduplicationService deduplication,
        ITelemetryEventPublisher publisher)
    {
        _deduplication = deduplication;
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(
        ReceiveTelemetryRequest request,
        CancellationToken cancellationToken)
    {
        var isNew = await _deduplication.TryRegisterAsync(
            request.VehicleId,
            request.Latitude,
            request.Longitude,
            request.Timestamp,
            cancellationToken);

        if (!isNew)
        {
            return Ok(new
            {
                status = "duplicate",
                message = "Telemetry event already received."
            });
        }

        var telemetryEvent = new TelemetryReceived(
            EventId: Guid.NewGuid(),
            VehicleId: request.VehicleId,
            Latitude: request.Latitude,
            Longitude: request.Longitude,
            RecordedAtUtc: request.Timestamp.ToUniversalTime(),
            ReceivedAtUtc: DateTime.UtcNow);

        await _publisher.PublishAsync(
            telemetryEvent,
            cancellationToken);

        return Accepted(new
        {
            telemetryEvent.EventId,
            status = "accepted"
        });
    }
}