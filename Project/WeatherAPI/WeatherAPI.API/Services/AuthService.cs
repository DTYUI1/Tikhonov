using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories.Interfaces;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to register user with email: {Email}", dto.Email);

        if (await _userRepository.EmailExistsAsync(dto.Email, cancellationToken))
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email.ToLower(),
            PasswordHash = HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = "User",
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(
            _configuration.GetValue<int>("Jwt:ExpirationHours", 24));

        return new AuthResponseDto(token, expiresAt, MapToUserResponse(user));
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

        var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found for email: {Email}", dto.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var hashedInput = HashPassword(dto.Password);
        _logger.LogDebug("Comparing hashes for user {Email}", dto.Email);
        
        if (user.PasswordHash != hashedInput)
        {
            _logger.LogWarning("Password mismatch for email: {Email}", dto.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is deactivated");
        }

        // Обновляем время последнего входа
        try
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            // Логируем, но не прерываем логин из-за ошибки обновления
            _logger.LogWarning(ex, "Failed to update LastLoginAt for user {UserId}", user.Id);
        }

        _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(
            _configuration.GetValue<int>("Jwt:ExpirationHours", 24));

        return new AuthResponseDto(token, expiresAt, MapToUserResponse(user));
    }

    public async Task<UserResponseDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        return MapToUserResponse(user);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] 
                ?? throw new InvalidOperationException("JWT Key not configured")));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:ExpirationHours", 24)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = $"{password}_WeatherAPI_Salt_2024";
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(bytes);
    }

    private static UserResponseDto MapToUserResponse(User user)
    {
        return new UserResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsActive,
            user.CreatedAt
        );
    }
}