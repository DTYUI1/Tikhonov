using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class CitiesController : BaseController
{
    private readonly ICityService _cityService;
    private readonly ILogger<CitiesController> _logger;

    public CitiesController(ICityService cityService, ILogger<CitiesController> logger)
    {
        _cityService = cityService;
        _logger = logger;
    }

    /// <summary>
    /// Get all cities with pagination and filtering
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get all cities with pagination and filtering")]
    [ProducesResponseType(typeof(PagedResponse<CityResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<CityResponseDto>>> GetAll(
        [FromQuery] CityFilterQuery filter,
        CancellationToken cancellationToken)
    {
        var result = await _cityService.GetAllAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get city by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get city by ID with weather types")]
    [ProducesResponseType(typeof(CityDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CityDetailDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _cityService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new city
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [SwaggerOperation(Summary = "Create a new city")]
    [ProducesResponseType(typeof(CityResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CityResponseDto>> Create(
        [FromBody] CityCreateDto dto,
        CancellationToken cancellationToken)
    {
        var userRole = GetUserRole();
        var result = await _cityService.CreateAsync(dto, userRole, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update a city
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [SwaggerOperation(Summary = "Update a city")]
    [ProducesResponseType(typeof(CityResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CityResponseDto>> Update(
        Guid id,
        [FromBody] CityUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var userRole = GetUserRole();
        var result = await _cityService.UpdateAsync(id, dto, userRole, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a city
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Delete a city")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userRole = GetUserRole();
        await _cityService.DeleteAsync(id, userRole, cancellationToken);
        return NoContent();
    }
}