using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Repositories.Interfaces;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class WeatherRecordsController : BaseController
{
    private readonly IWeatherRecordService _weatherRecordService;

    public WeatherRecordsController(IWeatherRecordService weatherRecordService)
    {
        _weatherRecordService = weatherRecordService;
    }

    /// <summary>
    /// Get all weather records with pagination and filtering
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get all weather records with pagination and filtering")]
    [ProducesResponseType(typeof(PagedResponse<WeatherRecordResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<WeatherRecordResponseDto>>> GetAll(
        [FromQuery] WeatherRecordFilterQuery filter,
        CancellationToken cancellationToken)
    {
        var result = await _weatherRecordService.GetAllAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get weather record by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get weather record by ID")]
    [ProducesResponseType(typeof(WeatherRecordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherRecordResponseDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _weatherRecordService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get current weather for a city
    /// </summary>
    [HttpGet("current/{cityId:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get current weather for a city")]
    [ProducesResponseType(typeof(CurrentWeatherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrentWeatherDto>> GetCurrentWeather(
        Guid cityId,
        CancellationToken cancellationToken)
    {
        var result = await _weatherRecordService.GetCurrentWeatherAsync(cityId, cancellationToken);
        
        if (result == null)
        {
            return NotFound(new ErrorResponse("NotFound", "No weather data available for this city", HttpContext.TraceIdentifier));
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Get weather statistics for a city
    /// </summary>
    [HttpGet("statistics/{cityId:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get weather statistics for a city")]
    [ProducesResponseType(typeof(WeatherStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WeatherStatisticsDto>> GetStatistics(
        Guid cityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow;

        var result = await _weatherRecordService.GetStatisticsAsync(cityId, fromDate, toDate, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get daily averages for a city
    /// </summary>
    [HttpGet("daily-averages/{cityId:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get daily temperature averages for a city")]
    [ProducesResponseType(typeof(IEnumerable<DailyAverageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DailyAverageDto>>> GetDailyAverages(
        Guid cityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to ?? DateTime.UtcNow;

        var result = await _weatherRecordService.GetDailyAveragesAsync(cityId, fromDate, toDate, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new weather record
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [SwaggerOperation(Summary = "Create a new weather record")]
    [ProducesResponseType(typeof(WeatherRecordResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WeatherRecordResponseDto>> Create(
        [FromBody] WeatherRecordCreateDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _weatherRecordService.CreateAsync(dto, GetUserRole(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update a weather record
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [SwaggerOperation(Summary = "Update a weather record")]
    [ProducesResponseType(typeof(WeatherRecordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherRecordResponseDto>> Update(
        Guid id,
        [FromBody] WeatherRecordUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _weatherRecordService.UpdateAsync(id, dto, GetUserRole(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a weather record
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Delete a weather record")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _weatherRecordService.DeleteAsync(id, GetUserRole(), cancellationToken);
        return NoContent();
    }
}