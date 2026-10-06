using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fleet.Infrastructure.Mongo;

public sealed class TelemetryDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid EventId { get; set; }

    public string VehicleId { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public DateTime RecordedAtUtc { get; set; }

    public DateTime ReceivedAtUtc { get; set; }
}