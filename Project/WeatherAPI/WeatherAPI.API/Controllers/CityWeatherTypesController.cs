using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class CityWeatherTypesController : BaseController
{
    private readonly ICityWeatherTypeService _cityWeatherTypeService;

    public CityWeatherTypesController(ICityWeatherTypeService cityWeatherTypeService)
    {
        _cityWeatherTypeService = cityWeatherTypeService;
    }

    /// <summary>
    /// Get weather types for a city
    /// </summary>
    [HttpGet("city/{cityId:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get all weather types associated with a city")]
    [ProducesResponseType(typeof(IEnumerable<CityWeatherTypeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CityWeatherTypeResponseDto>>> GetByCityId(
        Guid cityId,
        CancellationToken cancellationToken)
    {
        var result = await _cityWeatherTypeService.GetByCityIdAsync(cityId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get cities for a weather type
    /// </summary>
    [HttpGet("weathertype/{weatherTypeId:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get all cities associated with a weather type")]
    [ProducesResponseType(typeof(IEnumerable<CityWeatherTypeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CityWeatherTypeResponseDto>>> GetByWeatherTypeId(
        Guid weatherTypeId,
        CancellationToken cancellationToken)
    {
        var result = await _cityWeatherTypeService.GetByWeatherTypeIdAsync(weatherTypeId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Associate a weather type with a city
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [SwaggerOperation(Summary = "Associate a weather type with a city")]
    [ProducesResponseType(typeof(CityWeatherTypeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CityWeatherTypeResponseDto>> Create(
        [FromBody] CityWeatherTypeCreateDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _cityWeatherTypeService.CreateAsync(dto, GetUserRole(), cancellationToken);
        return CreatedAtAction(
            nameof(GetByCityId), 
            new { cityId = result.CityId }, 
            result);
    }

    /// <summary>
    /// Update city-weather type association
    /// </summary>
    [HttpPut("{cityId:guid}/{weatherTypeId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [SwaggerOperation(Summary = "Update city-weather type association")]
    [ProducesResponseType(typeof(CityWeatherTypeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CityWeatherTypeResponseDto>> Update(
        Guid cityId,
        Guid weatherTypeId,
        [FromBody] CityWeatherTypeUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _cityWeatherTypeService.UpdateAsync(
            cityId, weatherTypeId, dto, GetUserRole(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Remove city-weather type association
    /// </summary>
    [HttpDelete("{cityId:guid}/{weatherTypeId:guid}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Remove city-weather type association")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid cityId,
        Guid weatherTypeId,
        CancellationToken cancellationToken)
    {
        await _cityWeatherTypeService.DeleteAsync(cityId, weatherTypeId, GetUserRole(), cancellationToken);
        return NoContent();
    }
}