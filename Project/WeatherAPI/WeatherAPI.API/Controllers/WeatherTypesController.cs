using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class WeatherTypesController : BaseController
{
    private readonly IWeatherTypeService _weatherTypeService;

    public WeatherTypesController(IWeatherTypeService weatherTypeService)
    {
        _weatherTypeService = weatherTypeService;
    }

    /// <summary>
    /// Get all weather types
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get all weather types")]
    [ProducesResponseType(typeof(IEnumerable<WeatherTypeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WeatherTypeResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _weatherTypeService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get weather type by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get weather type by ID")]
    [ProducesResponseType(typeof(WeatherTypeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherTypeResponseDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _weatherTypeService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new weather type
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [SwaggerOperation(Summary = "Create a new weather type")]
    [ProducesResponseType(typeof(WeatherTypeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WeatherTypeResponseDto>> Create(
        [FromBody] WeatherTypeCreateDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _weatherTypeService.CreateAsync(dto, GetUserRole(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update a weather type
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [SwaggerOperation(Summary = "Update a weather type")]
    [ProducesResponseType(typeof(WeatherTypeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherTypeResponseDto>> Update(
        Guid id,
        [FromBody] WeatherTypeUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _weatherTypeService.UpdateAsync(id, dto, GetUserRole(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a weather type
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Delete a weather type")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _weatherTypeService.DeleteAsync(id, GetUserRole(), cancellationToken);
        return NoContent();
    }
}