using System.Text.Json;
using Fleet.Application.Abstractions;
using Fleet.Application.Models;
using StackExchange.Redis;

namespace Fleet.Infrastructure.Redis;

public sealed class RedisLatestVehicleStateCache
    : ILatestVehicleStateCache
{
    private readonly IDatabase _database;

    private static readonly TimeSpan Expiration =
        TimeSpan.FromMinutes(5);

    public RedisLatestVehicleStateCache(
        IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task SetAsync(
        LatestVehicleState state,
        CancellationToken cancellationToken = default)
    {
        var key = GetKey(state.VehicleId);

        var json = JsonSerializer.Serialize(state);

        await _database.StringSetAsync(
            key,
            json,
            Expiration);
    }

    public async Task<LatestVehicleState?> GetAsync(
        string vehicleId,
        CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(
            GetKey(vehicleId));

        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<LatestVehicleState>(
            value.ToString());
    }

    public async Task RemoveAsync(
        string vehicleId,
        CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(
            GetKey(vehicleId));
    }

    private static string GetKey(string vehicleId)
        => $"vehicle:{vehicleId}:latest";
}