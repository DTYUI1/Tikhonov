using System.Security.Cryptography;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories.Interfaces;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly ILogger<ApiKeyService> _logger;

    public ApiKeyService(IApiKeyRepository apiKeyRepository, ILogger<ApiKeyService> logger)
    {
        _apiKeyRepository = apiKeyRepository;
        _logger = logger;
    }

    public async Task<ApiKeyResponseDto> CreateAsync(
        ApiKeyCreateDto dto, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating API key for: {Name}", dto.Name);

        var key = GenerateApiKey();

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            Key = key,
            Name = dto.Name,
            UserId = dto.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(dto.ExpiresInDays),
            IsActive = true,
            Permissions = dto.Permissions != null 
                ? System.Text.Json.JsonSerializer.Serialize(dto.Permissions) 
                : null
        };

        await _apiKeyRepository.AddAsync(apiKey, cancellationToken);

        _logger.LogInformation("API key created: {ApiKeyId}", apiKey.Id);

        return new ApiKeyResponseDto(
            apiKey.Id,
            apiKey.Key,
            apiKey.Name,
            apiKey.UserId,
            apiKey.ExpiresAt,
            apiKey.IsActive,
            apiKey.CreatedAt
        );
    }

    public async Task<ApiKeyResponseDto?> ValidateAsync(
        string key, 
        CancellationToken cancellationToken = default)
    {
        var apiKey = await _apiKeyRepository.GetByKeyAsync(key, cancellationToken);

        if (apiKey == null)
        {
            _logger.LogWarning("API key not found");
            return null;
        }

        if (!apiKey.IsActive)
        {
            _logger.LogWarning("API key is deactivated: {ApiKeyId}", apiKey.Id);
            return null;
        }

        if (apiKey.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("API key expired: {ApiKeyId}", apiKey.Id);
            return null;
        }

        return new ApiKeyResponseDto(
            apiKey.Id,
            apiKey.Key,
            apiKey.Name,
            apiKey.UserId,
            apiKey.ExpiresAt,
            apiKey.IsActive,
            apiKey.CreatedAt
        );
    }

    public async Task<IEnumerable<ApiKeyResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var keys = await _apiKeyRepository.GetActiveKeysAsync(cancellationToken);

        return keys.Select(k => new ApiKeyResponseDto(
            k.Id,
            "***" + k.Key[^4..], // Показываем только последние 4 символа
            k.Name,
            k.UserId,
            k.ExpiresAt,
            k.IsActive,
            k.CreatedAt
        ));
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"API key with id {id} not found");

        apiKey.IsActive = false;
        await _apiKeyRepository.UpdateAsync(apiKey, cancellationToken);

        _logger.LogInformation("API key deactivated: {ApiKeyId}", id);
    }

    private static string GenerateApiKey()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_")[..43];
    }
}