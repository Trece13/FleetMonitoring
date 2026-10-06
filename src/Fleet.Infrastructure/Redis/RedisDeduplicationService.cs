using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Fleet.Application.Abstractions;
using StackExchange.Redis;

namespace Fleet.Infrastructure.Redis;

public sealed class RedisDeduplicationService : IDeduplicationService
{
    private readonly IDatabase _database;

    private static readonly TimeSpan DeduplicationWindow =
        TimeSpan.FromSeconds(10);

    public RedisDeduplicationService(
        IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<bool> TryRegisterAsync(
        string vehicleId,
        double latitude,
        double longitude,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var rawFingerprint = string.Join(
            "|",
            vehicleId,
            latitude.ToString("R", CultureInfo.InvariantCulture),
            longitude.ToString("R", CultureInfo.InvariantCulture),
            timestamp.ToUniversalTime().ToString("O"));

        var fingerprintBytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(rawFingerprint));

        var fingerprint =
            Convert.ToHexString(fingerprintBytes);

        var key = $"telemetry:dedupe:{fingerprint}";

        return await _database.StringSetAsync(
            key,
            "1",
            DeduplicationWindow,
            When.NotExists);
    }
}