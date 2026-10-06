namespace Fleet.Domain.Entities;

public sealed class Vehicle
{
    public Guid Id { get; private set; }

    public string ExternalId { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    private Vehicle()
    {
    }

    public Vehicle(string externalId, string name)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException(
                "External id is required.",
                nameof(externalId));

        Id = Guid.NewGuid();
        ExternalId = externalId;
        Name = name;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (!IsActive)
            return;

        IsActive = false;
        DeletedAtUtc = DateTime.UtcNow;
    }
}