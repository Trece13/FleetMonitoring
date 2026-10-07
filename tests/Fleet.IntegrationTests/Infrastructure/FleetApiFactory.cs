using Fleet.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fleet.IntegrationTests.Infrastructure;

public sealed class FleetApiFactory
    : WebApplicationFactory<Program>
{
    public FakeDeduplicationService Deduplication { get; } =
        new();

    public FakeTelemetryEventPublisher Publisher { get; } =
        new();

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDeduplicationService>();
            services.RemoveAll<ITelemetryEventPublisher>();

            services.AddSingleton<IDeduplicationService>(
                Deduplication);

            services.AddSingleton<ITelemetryEventPublisher>(
                Publisher);
        });
    }

    public void Reset()
    {
        Deduplication.Clear();
        Publisher.Clear();
    }
}