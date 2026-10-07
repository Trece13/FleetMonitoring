using Fleet.Contracts.Telemetry;
using MongoDB.Driver;
using Fleet.Application.Abstractions;

namespace Fleet.Infrastructure.Mongo;

public sealed class TelemetryMongoRepository : ITelemetryHistoryQueries
{
    private readonly IMongoCollection<TelemetryDocument>
        _collection;

    public TelemetryMongoRepository(
        IMongoDatabase database)
    {
        _collection =
            database.GetCollection<TelemetryDocument>(
                "telemetry");

        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var eventIndex =
            new CreateIndexModel<TelemetryDocument>(
                Builders<TelemetryDocument>
                    .IndexKeys
                    .Ascending(x => x.EventId),
                new CreateIndexOptions
                {
                    Unique = true
                });

        var historyIndex =
            new CreateIndexModel<TelemetryDocument>(
                Builders<TelemetryDocument>
                    .IndexKeys
                    .Ascending(x => x.VehicleId)
                    .Descending(x => x.RecordedAtUtc));

        _collection.Indexes.CreateMany(
            new[]
            {
                eventIndex,
                historyIndex
            });
    }

    public async Task InsertIfNotExistsAsync(
        TelemetryReceived telemetry,
        CancellationToken cancellationToken)
    {
        var document =
            new TelemetryDocument
            {
                EventId = telemetry.EventId,
                VehicleId = telemetry.VehicleId,
                Latitude = telemetry.Latitude,
                Longitude = telemetry.Longitude,
                RecordedAtUtc =
                    telemetry.RecordedAtUtc,
                ReceivedAtUtc =
                    telemetry.ReceivedAtUtc
            };

        try
        {
            await _collection.InsertOneAsync(
                document,
                cancellationToken:
                    cancellationToken);
        }
        catch (MongoWriteException exception)
            when (
                exception.WriteError.Category ==
                ServerErrorCategory.DuplicateKey)
        {
            // Idempotencia.
            //
            // Si RabbitMQ vuelve a entregar el mismo
            // evento no queremos duplicar el histórico.
        }
    }
    public async Task<IReadOnlyList<TelemetryHistoryItem>> GetByVehicleAsync(
    string vehicleId,
    int limit = 100,
    CancellationToken cancellationToken = default)
    {
        var documents = await _collection
            .Find(x => x.VehicleId == vehicleId)
            .SortByDescending(x => x.RecordedAtUtc)
            .Limit(limit)
            .ToListAsync(cancellationToken);

        return documents
            .Select(x => new TelemetryHistoryItem(
                x.EventId,
                x.Latitude,
                x.Longitude,
                x.RecordedAtUtc))
            .ToList();
    }
    public async Task DeleteByVehicleAsync(
    string vehicleId,
    CancellationToken cancellationToken = default)
    {
        await _collection.DeleteManyAsync(
            x => x.VehicleId == vehicleId,
            cancellationToken);
    }
}