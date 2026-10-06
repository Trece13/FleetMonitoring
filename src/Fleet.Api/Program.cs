using Fleet.Api.Consumers;
using Fleet.Api.Hubs;
using Fleet.Application;
using Fleet.Application.Abstractions;
using Fleet.Infrastructure;
using Fleet.Infrastructure.Messaging;
using FluentValidation.AspNetCore;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// OpenAPI + Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();
builder.Services.AddSignalR();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddScoped<
    ITelemetryEventPublisher,
    TelemetryEventPublisher>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<VehicleStateUpdatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMq:Host"] ?? "localhost",
            "/",
            h =>
            {
                h.Username(
                    builder.Configuration["RabbitMq:Username"] ?? "fleet");

                h.Password(
                    builder.Configuration["RabbitMq:Password"] ?? "fleet123");
            });

        cfg.ReceiveEndpoint(
            "fleet-realtime",
            endpoint =>
            {
                endpoint.ConfigureConsumer<
                    VehicleStateUpdatedConsumer>(context);
            });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // OpenAPI nativo
    app.MapOpenApi();

    // Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.MapControllers();

app.MapHealthChecks("/health");

app.MapHub<FleetHub>("/hubs/fleet");

app.Run();

public partial class Program;