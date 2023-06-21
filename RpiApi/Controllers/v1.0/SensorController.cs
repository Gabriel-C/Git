namespace RpiApi.Controllers.v1_0;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RpiApi.Repository.v1_0;

[ApiController]
[ApiVersion(1.0)]
[Route("v{version:apiVersion}/[controller]/[action]")]
[ApiExplorerSettings(GroupName = "Sensors")]
[Produces("application/json", "text/plain")]
[Consumes("application/json")]
public class SensorController : ControllerBase
{
    private readonly ISensorsDa _sensors;

    public SensorController(ISensorsDa sensors)
    {
        _sensors = sensors;
    }

    [HttpGet]
    [MapToApiVersion(1.0)]
    [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public IActionResult GetTemperature()
    {
        try
        {
            double temperature = _sensors.GetTemperature();
            return Ok(temperature);
        }
        catch(Exception e)
        {
            return StatusCode(500,e.Message);
        }
    }
}