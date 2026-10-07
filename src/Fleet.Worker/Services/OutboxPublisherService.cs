using System.Text.Json;
using Fleet.Contracts.Vehicles;
using Fleet.Infrastructure.SqlServer;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Fleet.Worker.Services;

public sealed class OutboxPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger;

    public OutboxPublisherService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox publisher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing outbox messages");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(2),
                stoppingToken);
        }
    }

    private async Task ProcessMessagesAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<FleetDbContext>();

        var publisher =
            scope.ServiceProvider
                .GetRequiredService<IPublishEndpoint>();

        var messages =
            await dbContext.OutboxMessages
                .Where(x => x.ProcessedAtUtc == null)
                .OrderBy(x => x.OccurredAtUtc)
                .Take(20)
                .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                switch (message.Type)
                {
                    case nameof(VehicleDeleted):

                        var vehicleDeleted =
                            JsonSerializer.Deserialize<VehicleDeleted>(
                                message.Payload);

                        if (vehicleDeleted is null)
                        {
                            _logger.LogError(
                                "Could not deserialize outbox message {OutboxId}",
                                message.Id);

                            continue;
                        }

                        await publisher.Publish(
                            vehicleDeleted,
                            cancellationToken);

                        break;

                    default:

                        _logger.LogWarning(
                            "Unknown outbox message type {MessageType}",
                            message.Type);

                        continue;
                }

                message.MarkAsProcessed();

                await dbContext.SaveChangesAsync(
                    cancellationToken);

                _logger.LogInformation(
                    "Outbox message {OutboxId} published",
                    message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Could not publish outbox message {OutboxId}",
                    message.Id);

                // No marcamos ProcessedAtUtc.
                // Se intentará nuevamente.
            }
        }
    }
}