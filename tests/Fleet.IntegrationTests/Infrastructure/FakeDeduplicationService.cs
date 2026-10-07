using System.Collections.Concurrent;
using System.Globalization;
using Fleet.Application.Abstractions;

namespace Fleet.IntegrationTests.Infrastructure;

public sealed class FakeDeduplicationService
    : IDeduplicationService
{
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public Task<bool> TryRegisterAsync(
        string vehicleId,
        double latitude,
        double longitude,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var key = string.Join(
            "|",
            vehicleId,
            latitude.ToString("R", CultureInfo.InvariantCulture),
            longitude.ToString("R", CultureInfo.InvariantCulture),
            timestamp.ToUniversalTime().ToString("O"));

        var registered = _keys.TryAdd(key, 0);

        return Task.FromResult(registered);
    }

    public void Clear()
    {
        _keys.Clear();
    }
}