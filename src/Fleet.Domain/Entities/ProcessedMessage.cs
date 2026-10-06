namespace Fleet.Domain.Entities;

public sealed class ProcessedMessage
{
    public Guid EventId { get; private set; }

    public DateTime ProcessedAtUtc { get; private set; }

    private ProcessedMessage()
    {
    }

    public ProcessedMessage(Guid eventId)
    {
        EventId = eventId;

        ProcessedAtUtc = DateTime.UtcNow;
    }
}