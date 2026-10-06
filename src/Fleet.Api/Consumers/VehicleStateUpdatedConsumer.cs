using Fleet.Api.Hubs;
using Fleet.Contracts.Realtime;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fleet.Api.Consumers;

public sealed class VehicleStateUpdatedConsumer
    : IConsumer<VehicleStateUpdated>
{
    private readonly IHubContext<FleetHub> _hubContext;
    private readonly ILogger<VehicleStateUpdatedConsumer> _logger;

    public VehicleStateUpdatedConsumer(
        IHubContext<FleetHub> hubContext,
        ILogger<VehicleStateUpdatedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(
        ConsumeContext<VehicleStateUpdated> context)
    {
        var state = context.Message;

        await _hubContext.Clients.All.SendAsync(
            "VehicleStateUpdated",
            state,
            context.CancellationToken);

        _logger.LogInformation(
            "Realtime update sent for {VehicleId}",
            state.VehicleId);
    }
}