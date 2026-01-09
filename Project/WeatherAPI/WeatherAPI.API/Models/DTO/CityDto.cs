namespace WeatherAPI.API.Models.DTO;

public record CityCreateDto(
    string Name,
    string Country,
    double Latitude,
    double Longitude,
    string? TimeZone
);

public record CityUpdateDto(
    string Name,
    string Country,
    double Latitude,
    double Longitude,
    string? TimeZone
);

public record CityResponseDto(
    Guid Id,
    string Name,
    string Country,
    double Latitude,
    double Longitude,
    string? TimeZone,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CityDetailDto(
    Guid Id,
    string Name,
    string Country,
    double Latitude,
    double Longitude,
    string? TimeZone,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<WeatherTypeFrequencyDto> CommonWeatherTypes
);

public record WeatherTypeFrequencyDto(
    Guid WeatherTypeId,
    string WeatherTypeName,
    int Frequency,
    string? Season
);