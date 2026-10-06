using Fleet.Contracts.Telemetry;
using Fleet.Domain.Entities;
using Fleet.Infrastructure.Mongo;
using Fleet.Infrastructure.SqlServer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Fleet.Application.Abstractions;
using Fleet.Application.Models;
using Fleet.Contracts.Realtime;

namespace Fleet.Worker.Consumers;

public sealed class TelemetryConsumer
    : IConsumer<TelemetryReceived>
{
    private readonly FleetDbContext _dbContext;

    private readonly TelemetryMongoRepository
        _mongoRepository;

    private readonly ILogger<TelemetryConsumer>
        _logger;

    private readonly ILatestVehicleStateCache _latestStateCache;
    public TelemetryConsumer(
        FleetDbContext dbContext,
        TelemetryMongoRepository mongoRepository,
        ILatestVehicleStateCache latestStateCache,
        ILogger<TelemetryConsumer> logger)
    {
        _dbContext = dbContext;
        _mongoRepository = mongoRepository;
        _latestStateCache = latestStateCache;
        _logger = logger;
    }

    public async Task Consume(
        ConsumeContext<TelemetryReceived> context)
    {
        var telemetry = context.Message;
        var stateWasUpdated = false;

        _logger.LogInformation(
            "Processing telemetry {EventId} for {VehicleId}",
            telemetry.EventId,
            telemetry.VehicleId);

        //
        // 1. ¿Ya quedó completamente procesado?
        //
        var alreadyProcessed =
            await _dbContext
                .ProcessedMessages
                .AnyAsync(
                    x =>
                        x.EventId ==
                        telemetry.EventId,
                    context.CancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogInformation(
                "Telemetry {EventId} was already processed",
                telemetry.EventId);

            return;
        }

        //
        // 2. Histórico Mongo.
        //
        await _mongoRepository
            .InsertIfNotExistsAsync(
                telemetry,
                context.CancellationToken);

        //
        // 3. Comienza la parte transaccional SQL.
        //
        await using var transaction =
            await _dbContext
                .Database
                .BeginTransactionAsync(
                    context.CancellationToken);

        try
        {
            var vehicle =
                await _dbContext
                    .Vehicles
                    .SingleOrDefaultAsync(
                        x =>
                            x.ExternalId ==
                            telemetry.VehicleId,
                        context.CancellationToken);

            if (vehicle is null)
            {
                vehicle =
                    new Vehicle(
                        telemetry.VehicleId,
                        telemetry.VehicleId);

                _dbContext.Vehicles.Add(
                    vehicle);
            }

            var state =
                await _dbContext
                    .VehicleStates
                    .SingleOrDefaultAsync(
                        x =>
                            x.VehicleId ==
                            vehicle.Id,
                        context.CancellationToken);

            if (state is null)
            {
                state =
                    new VehicleState(
                        vehicle.Id,
                        telemetry.Latitude,
                        telemetry.Longitude,
                        telemetry.RecordedAtUtc);

                _dbContext.VehicleStates.Add(
                    state);
                stateWasUpdated = true;
            }
            else
            {
                var result =
                    state.ApplyPosition(
                        telemetry.Latitude,
                        telemetry.Longitude,
                        telemetry.RecordedAtUtc);
                
                if (!result.IsStale)
                {
                    stateWasUpdated = true;
                }
                if (
                    result
                        .StopAlertShouldBeCreated)
                {
                    var alert =
                        Alert.VehicleStopped(
                            vehicle.Id,
                            telemetry.RecordedAtUtc);

                    _dbContext.Alerts.Add(
                        alert);

                    _logger.LogWarning(
                        "Vehicle {VehicleId} stopped. Alert created.",
                        telemetry.VehicleId);
                }
            }

            _dbContext.ProcessedMessages.Add(
                new ProcessedMessage(
                    telemetry.EventId));

            await _dbContext.SaveChangesAsync(
                context.CancellationToken);

            await transaction.CommitAsync(
                context.CancellationToken);

            _logger.LogInformation(
            "StateWasUpdated={StateWasUpdated} for {VehicleId}",
            stateWasUpdated,
            telemetry.VehicleId);

            if (stateWasUpdated)
            {
                var latestState = new LatestVehicleState(
                    telemetry.VehicleId,
                    state.Latitude,
                    state.Longitude,
                    state.Status.ToString(),
                    state.LastSeenAtUtc);

                try
                {
                    await _latestStateCache.SetAsync(
                        latestState,
                        context.CancellationToken);

                    _logger.LogInformation(
                        "Redis latest state SAVED for {VehicleId}",
                        telemetry.VehicleId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Could not update Redis latest state for {VehicleId}",
                        telemetry.VehicleId);
                }

                await context.Publish(
                    new VehicleStateUpdated(
                        telemetry.VehicleId,
                        state.Latitude,
                        state.Longitude,
                        state.Status.ToString(),
                        state.LastSeenAtUtc),
                    context.CancellationToken);
            }
        }
        catch
        {
            await transaction.RollbackAsync(
                context.CancellationToken);

            throw;
        }
    }
}