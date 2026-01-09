namespace WeatherAPI.API.Models.DTO;

public record ApiKeyCreateDto(
    string Name,
    Guid? UserId,
    int ExpiresInDays,
    List<string>? Permissions
);

public record ApiKeyResponseDto(
    Guid Id,
    string Key,
    string Name,
    Guid? UserId,
    DateTime ExpiresAt,
    bool IsActive,
    DateTime CreatedAt
);