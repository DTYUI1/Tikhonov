using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}