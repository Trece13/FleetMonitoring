using Fleet.Application.Abstractions;
using Fleet.Infrastructure.Messaging;
using Fleet.Infrastructure.Mongo;
using Fleet.Infrastructure.Redis;
using Fleet.Infrastructure.SqlServer;
using Fleet.Infrastructure.Queries;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;
using Fleet.Infrastructure.Commands;

namespace Fleet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddSqlServer(
            services,
            configuration);

        AddMongoDb(
            services,
            configuration);

        AddRedis(
            services,
            configuration);

        //AddMessaging(
        //    services,
        //    configuration);

        services.AddScoped<
            IDeduplicationService,
            RedisDeduplicationService>();

        //services.AddScoped<
        //    ITelemetryEventPublisher,
        //    TelemetryEventPublisher>();

        services.AddScoped<
            TelemetryMongoRepository>();

        services.AddScoped<
            ITelemetryHistoryQueries,
            TelemetryMongoRepository>();

        services.AddScoped<
            ILatestVehicleStateCache,
            RedisLatestVehicleStateCache>();

        services.AddScoped<
            IVehicleQueries,
            VehicleQueries>();

        services.AddScoped<
            IAlertQueries, 
            AlertQueries>();

        services.AddScoped<
            IVehicleCommands,
            VehicleCommands>();

        return services;
    }

    private static void AddSqlServer(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "SqlServer")
            ?? throw new InvalidOperationException(
                "SQL Server connection string missing.");

        services.AddDbContext<FleetDbContext>(
            options =>
                options.UseSqlServer(
                    connectionString));
    }

    private static void AddMongoDb(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "MongoDb")
            ?? throw new InvalidOperationException(
                "MongoDB connection string missing.");

        var databaseName =
            configuration[
                "MongoDb:DatabaseName"]
            ?? "FleetTelemetry";

        services.AddSingleton<IMongoClient>(
            new MongoClient(
                connectionString));

        services.AddSingleton(
            serviceProvider =>
            {
                var client =
                    serviceProvider
                        .GetRequiredService<
                            IMongoClient>();

                return client.GetDatabase(
                    databaseName);
            });
    }

    private static void AddRedis(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "Redis")
            ?? throw new InvalidOperationException(
                "Redis connection string missing.");

        services.AddSingleton<
            IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(
                    connectionString));
    }

    private static void AddMessaging(
        IServiceCollection services,
        IConfiguration configuration)
    {
        //services.AddMassTransit(bus =>
        //{
        //    bus.UsingRabbitMq(
        //        (context, cfg) =>
        //        {
        //            cfg.Host(
        //                configuration[
        //                    "RabbitMq:Host"]
        //                ?? "localhost",
        //                "/",
        //                host =>
        //                {
        //                    host.Username(
        //                        configuration[
        //                            "RabbitMq:Username"]
        //                        ?? "fleet");

        //                    host.Password(
        //                        configuration[
        //                            "RabbitMq:Password"]
        //                        ?? "fleet123");
        //                });
        //        });
        //});
    }
}