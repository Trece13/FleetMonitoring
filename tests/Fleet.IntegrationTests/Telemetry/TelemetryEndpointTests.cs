using System.Net;
using System.Net.Http.Json;
using Fleet.Contracts.Telemetry;
using Fleet.IntegrationTests.Infrastructure;

namespace Fleet.IntegrationTests.Telemetry;

public sealed class TelemetryEndpointTests
    : IClassFixture<FleetApiFactory>
{
    private readonly FleetApiFactory _factory;
    private readonly HttpClient _client;

    public TelemetryEndpointTests(
        FleetApiFactory factory)
    {
        _factory = factory;

        _factory.Reset();

        _client = factory.CreateClient(
            new()
            {
                BaseAddress =
                    new Uri("https://localhost")
            });
    }


    [Fact]
    public async Task PostTelemetry_WithValidRequest_ShouldReturn202()
    {
        // Arrange
        var request =
            new ReceiveTelemetryRequest(
                VehicleId: "VEH-INTEGRATION-001",
                Latitude: 4.6500,
                Longitude: -74.0600,
                Timestamp: DateTime.UtcNow);

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/telemetry",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Accepted,
            response.StatusCode);
    }


    [Fact]
    public async Task PostTelemetry_WithValidRequest_ShouldPublishEvent()
    {
        var timestamp =
            DateTime.UtcNow;

        var request =
            new ReceiveTelemetryRequest(
                VehicleId: "VEH-INTEGRATION-002",
                Latitude: 4.6500,
                Longitude: -74.0600,
                Timestamp: timestamp);

        var response =
            await _client.PostAsJsonAsync(
                "/api/telemetry",
                request);


        Assert.Equal(
            HttpStatusCode.Accepted,
            response.StatusCode);


        var published =
            Assert.Single(
                _factory.Publisher.Published);


        Assert.Equal(
            request.VehicleId,
            published.VehicleId);

        Assert.Equal(
            request.Latitude,
            published.Latitude);

        Assert.Equal(
            request.Longitude,
            published.Longitude);

        Assert.NotEqual(
            Guid.Empty,
            published.EventId);
    }


    [Fact]
    public async Task PostTelemetry_WhenSameMessageIsSentTwice_ShouldNotPublishTwice()
    {
        // Muy importante:
        // exactamente el mismo timestamp.
        var timestamp =
            DateTime.UtcNow;

        var request =
            new ReceiveTelemetryRequest(
                VehicleId: "VEH-DUPLICATE-001",
                Latitude: 4.6500,
                Longitude: -74.0600,
                Timestamp: timestamp);


        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/telemetry",
                request);

        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/telemetry",
                request);


        Assert.Equal(
            HttpStatusCode.Accepted,
            firstResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.OK,
            secondResponse.StatusCode);


        Assert.Single(
            _factory.Publisher.Published);
    }


    [Fact]
    public async Task PostTelemetry_WithSameCoordinatesButDifferentTimestamp_ShouldBeAccepted()
    {
        var first =
            new ReceiveTelemetryRequest(
                VehicleId: "VEH-STATIONARY-001",
                Latitude: 4.6500,
                Longitude: -74.0600,
                Timestamp: DateTime.UtcNow);

        var second =
            first with
            {
                Timestamp =
                    first.Timestamp.AddSeconds(30)
            };


        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/telemetry",
                first);

        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/telemetry",
                second);


        Assert.Equal(
            HttpStatusCode.Accepted,
            firstResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.Accepted,
            secondResponse.StatusCode);


        Assert.Equal(
            2,
            _factory.Publisher.Published.Count);
    }


    [Theory]
    [InlineData(91, -74.06)]
    [InlineData(-91, -74.06)]
    [InlineData(4.65, 181)]
    [InlineData(4.65, -181)]
    public async Task PostTelemetry_WithInvalidCoordinates_ShouldReturn400(
        double latitude,
        double longitude)
    {
        var request =
            new ReceiveTelemetryRequest(
                VehicleId: "VEH-INVALID-001",
                Latitude: latitude,
                Longitude: longitude,
                Timestamp: DateTime.UtcNow);


        var response =
            await _client.PostAsJsonAsync(
                "/api/telemetry",
                request);


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Empty(
            _factory.Publisher.Published);
    }
}