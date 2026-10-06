using Fleet.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Fleet.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public sealed class AlertsController : ControllerBase
{
    private readonly IAlertQueries _alertQueries;

    public AlertsController(IAlertQueries alertQueries)
    {
        _alertQueries = alertQueries;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var alerts = await _alertQueries.GetAllAsync(
            cancellationToken);

        return Ok(alerts);
    }
}