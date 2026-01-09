namespace WeatherAPI.API.Models.DTO;

public record WeatherTypeCreateDto(
    string Name,
    string Description,
    string IconCode
);

public record WeatherTypeUpdateDto(
    string Name,
    string Description,
    string IconCode
);

public record WeatherTypeResponseDto(
    Guid Id,
    string Name,
    string Description,
    string IconCode,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);