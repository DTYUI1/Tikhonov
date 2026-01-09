namespace WeatherAPI.API.Models.DTO;

public record CityWeatherTypeCreateDto(
    Guid CityId,
    Guid WeatherTypeId,
    int Frequency,
    string? Season
);

public record CityWeatherTypeUpdateDto(
    int Frequency,
    string? Season
);

public record CityWeatherTypeResponseDto(
    Guid CityId,
    string CityName,
    Guid WeatherTypeId,
    string WeatherTypeName,
    int Frequency,
    string? Season
);