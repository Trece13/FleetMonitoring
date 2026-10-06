using System.Net.Http.Json;
using Fleet.Contracts.Telemetry;

var apiUrl = args.FirstOrDefault()
             ?? "https://localhost:7051";

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};

using var httpClient = new HttpClient(handler)
{
    BaseAddress = new Uri(apiUrl)
};

var random = new Random();

var vehicles = new List<SimulatedVehicle>
{
    new("VEH-001", 4.6500, -74.0600, true),
    new("VEH-002", 4.6510, -74.0610, true),
    new("VEH-003", 4.6520, -74.0620, true),
    new("VEH-004", 4.6530, -74.0630, true),

    // Este vehículo permanecerá detenido.
    new("VEH-005", 4.6540, -74.0640, false)
};

Console.WriteLine("Fleet Simulator");
Console.WriteLine($"API: {apiUrl}");
Console.WriteLine("Press Ctrl+C to stop.");
Console.WriteLine();

while (true)
{
    foreach (var vehicle in vehicles)
    {
        if (vehicle.IsMoving)
        {
            vehicle.Move(random);
        }

        var telemetry = new ReceiveTelemetryRequest(
            vehicle.VehicleId,
            vehicle.Latitude,
            vehicle.Longitude,
            DateTime.UtcNow);

        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "/api/telemetry",
                telemetry);

            Console.WriteLine(
                "{0:HH:mm:ss} {1} | {2:F6}, {3:F6} | HTTP {4}",
                DateTime.UtcNow,
                vehicle.VehicleId,
                vehicle.Latitude,
                vehicle.Longitude,
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "{0} | ERROR | {1}",
                vehicle.VehicleId,
                ex.Message);
        }
    }

    var delay = random.Next(2000, 5001);

    Console.WriteLine(
        $"Waiting {delay / 1000.0:F1}s...");
    Console.WriteLine();

    await Task.Delay(delay);
}

internal sealed class SimulatedVehicle
{
    public string VehicleId { get; }

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }

    public bool IsMoving { get; }

    public SimulatedVehicle(
        string vehicleId,
        double latitude,
        double longitude,
        bool isMoving)
    {
        VehicleId = vehicleId;
        Latitude = latitude;
        Longitude = longitude;
        IsMoving = isMoving;
    }

    public void Move(Random random)
    {
        // Aproximadamente 10–30 metros por actualización.
        var latitudeDelta =
            random.NextDouble() * 0.00020 + 0.00010;

        var longitudeDelta =
            random.NextDouble() * 0.00020 + 0.00010;

        Latitude += latitudeDelta;
        Longitude += longitudeDelta;
    }
}