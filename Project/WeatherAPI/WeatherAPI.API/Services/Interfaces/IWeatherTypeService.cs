using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Services.Interfaces;

public interface IWeatherTypeService
{
    Task<IEnumerable<WeatherTypeResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WeatherTypeResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WeatherTypeResponseDto> CreateAsync(WeatherTypeCreateDto dto, string userRole, CancellationToken cancellationToken = default);
    Task<WeatherTypeResponseDto> UpdateAsync(Guid id, WeatherTypeUpdateDto dto, string userRole, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string userRole, CancellationToken cancellationToken = default);
}