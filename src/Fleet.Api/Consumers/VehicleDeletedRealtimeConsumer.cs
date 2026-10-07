using Fleet.Api.Hubs;
using Fleet.Contracts.Vehicles;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fleet.Api.Consumers;

public sealed class VehicleDeletedRealtimeConsumer
    : IConsumer<VehicleDeleted>
{
    private readonly IHubContext<FleetHub> _hubContext;

    public VehicleDeletedRealtimeConsumer(
        IHubContext<FleetHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Consume(
        ConsumeContext<VehicleDeleted> context)
    {
        await _hubContext.Clients.All.SendAsync(
            "VehicleDeleted",
            context.Message.ExternalId,
            context.CancellationToken);
    }
}