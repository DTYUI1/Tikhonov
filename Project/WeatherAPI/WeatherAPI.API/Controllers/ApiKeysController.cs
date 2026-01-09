using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ApiKeysController : BaseController
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeysController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    /// <summary>
    /// Get all API keys
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all active API keys")]
    [ProducesResponseType(typeof(IEnumerable<ApiKeyResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApiKeyResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _apiKeyService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new API key
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create a new API key")]
    [ProducesResponseType(typeof(ApiKeyResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiKeyResponseDto>> Create(
        [FromBody] ApiKeyCreateDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _apiKeyService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetAll), result);
    }

    /// <summary>
    /// Deactivate an API key
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Deactivate an API key")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _apiKeyService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}