using Fleet.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Fleet.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleQueries _vehicleQueries;
    private readonly ITelemetryHistoryQueries _historyQueries;
    private readonly IVehicleCommands _vehicleCommands;

    public VehiclesController(
        IVehicleQueries vehicleQueries,
        ITelemetryHistoryQueries historyQueries,
        IVehicleCommands vehicleCommands)
    {
        _vehicleQueries = vehicleQueries;
        _historyQueries = historyQueries;
        _vehicleCommands = vehicleCommands;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var vehicles =
            await _vehicleQueries.GetAllAsync(
                cancellationToken);

        return Ok(vehicles);
    }

    [HttpGet("{vehicleId}")]
    public async Task<IActionResult> GetById(
        string vehicleId,
        CancellationToken cancellationToken)
    {
        var vehicle =
            await _vehicleQueries.GetByExternalIdAsync(
                vehicleId,
                cancellationToken);

        if (vehicle is null)
            return NotFound();

        return Ok(vehicle);
    }

    [HttpGet("{vehicleId}/history")]
    public async Task<IActionResult> GetHistory(
    string vehicleId,
    [FromQuery] int limit = 100,
    CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 500);

        var history = await _historyQueries.GetByVehicleAsync(
            vehicleId,
            limit,
            cancellationToken);

        return Ok(history);
    }

    [HttpDelete("{vehicleId}")]
    public async Task<IActionResult> Delete(
    string vehicleId,
    CancellationToken cancellationToken)
    {
        var deleted =
            await _vehicleCommands.DeleteAsync(
                vehicleId,
                cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

}