using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Services.Interfaces;

public interface ICityService
{
    Task<PagedResponse<CityResponseDto>> GetAllAsync(CityFilterQuery filter, CancellationToken cancellationToken = default);
    Task<CityDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CityResponseDto> CreateAsync(CityCreateDto dto, string userRole, CancellationToken cancellationToken = default);
    Task<CityResponseDto> UpdateAsync(Guid id, CityUpdateDto dto, string userRole, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string userRole, CancellationToken cancellationToken = default);
}