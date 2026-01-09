using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Services.Interfaces;

public interface IApiKeyService
{
    Task<ApiKeyResponseDto> CreateAsync(ApiKeyCreateDto dto, CancellationToken cancellationToken = default);
    Task<ApiKeyResponseDto?> ValidateAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiKeyResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}