using Fleet.Infrastructure;
using Fleet.Worker.Consumers;
using Fleet.Worker.Services;
using MassTransit;

var builder =
    Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddMassTransit(
    bus =>
    {
        bus.AddConsumer<TelemetryConsumer>();
        bus.AddConsumer<VehicleDeletedConsumer>();
        bus.UsingRabbitMq(
            (context, cfg) =>
            {
                cfg.Host(
                    builder.Configuration[
                        "RabbitMq:Host"]
                    ?? "localhost",
                    "/",
                    host =>
                    {
                        host.Username(
                            builder.Configuration[
                                "RabbitMq:Username"]
                            ?? "fleet");

                        host.Password(
                            builder.Configuration[
                                "RabbitMq:Password"]
                            ?? "fleet123");
                    });

                cfg.ReceiveEndpoint(
                    "fleet-telemetry",
                    endpoint =>
                    {
                        endpoint.UseMessageRetry(
                            retry =>
                            {
                                retry.Intervals(
                                    TimeSpan.FromSeconds(2),
                                    TimeSpan.FromSeconds(5),
                                    TimeSpan.FromSeconds(10));
                            });

                        endpoint.ConfigureConsumer<
                            TelemetryConsumer>(
                                context);
                    });

                cfg.ReceiveEndpoint(
                    "fleet-vehicle-deleted",
                    endpoint =>
                    {
                        endpoint.UseMessageRetry(r =>
                        {
                            r.Intervals(
                                TimeSpan.FromSeconds(2),
                                TimeSpan.FromSeconds(5),
                                TimeSpan.FromSeconds(10));
                        });

                        endpoint.ConfigureConsumer<VehicleDeletedConsumer>(
                            context);
                    });
            });
    });

builder.Services.AddHostedService<OutboxPublisherService>();
var host = builder.Build();

host.Run();