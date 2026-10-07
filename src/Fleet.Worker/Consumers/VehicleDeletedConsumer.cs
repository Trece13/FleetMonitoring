using Fleet.Application.Abstractions;
using Fleet.Contracts.Vehicles;
using Fleet.Infrastructure.Mongo;
using MassTransit;

namespace Fleet.Worker.Consumers;

public sealed class VehicleDeletedConsumer
    : IConsumer<VehicleDeleted>
{
    private readonly ILatestVehicleStateCache _cache;
    private readonly TelemetryMongoRepository _mongoRepository;
    private readonly ILogger<VehicleDeletedConsumer> _logger;

    public VehicleDeletedConsumer(
        ILatestVehicleStateCache cache,
        TelemetryMongoRepository mongoRepository,
        ILogger<VehicleDeletedConsumer> logger)
    {
        _cache = cache;
        _mongoRepository = mongoRepository;
        _logger = logger;
    }

    public async Task Consume(
        ConsumeContext<VehicleDeleted> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing deletion for vehicle {VehicleId}",
            message.ExternalId);

        await _cache.RemoveAsync(
            message.ExternalId,
            context.CancellationToken);

        await _mongoRepository.DeleteByVehicleAsync(
            message.ExternalId,
            context.CancellationToken);

        _logger.LogInformation(
            "Vehicle {VehicleId} removed from Redis and MongoDB",
            message.ExternalId);
    }
}