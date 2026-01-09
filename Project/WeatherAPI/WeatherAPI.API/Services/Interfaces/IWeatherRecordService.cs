using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Repositories.Interfaces;

namespace WeatherAPI.API.Services.Interfaces;

public interface IWeatherRecordService
{
    Task<PagedResponse<WeatherRecordResponseDto>> GetAllAsync(WeatherRecordFilterQuery filter, CancellationToken cancellationToken = default);
    Task<WeatherRecordResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CurrentWeatherDto?> GetCurrentWeatherAsync(Guid cityId, CancellationToken cancellationToken = default);
    Task<WeatherRecordResponseDto> CreateAsync(WeatherRecordCreateDto dto, string userRole, CancellationToken cancellationToken = default);
    Task<WeatherRecordResponseDto> UpdateAsync(Guid id, WeatherRecordUpdateDto dto, string userRole, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string userRole, CancellationToken cancellationToken = default);
    Task<WeatherStatisticsDto> GetStatisticsAsync(Guid cityId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyAverageDto>> GetDailyAveragesAsync(Guid cityId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}