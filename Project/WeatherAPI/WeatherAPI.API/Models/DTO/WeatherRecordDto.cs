namespace WeatherAPI.API.Models.DTO;

public record WeatherRecordCreateDto(
    Guid CityId,
    Guid WeatherTypeId,
    DateTime RecordedAt,
    double Temperature,
    double FeelsLike,
    int Humidity,
    double WindSpeed,
    int? WindDirection,
    int? Pressure,
    int? Visibility
);

public record WeatherRecordUpdateDto(
    Guid CityId,
    Guid WeatherTypeId,
    DateTime RecordedAt,
    double Temperature,
    double FeelsLike,
    int Humidity,
    double WindSpeed,
    int? WindDirection,
    int? Pressure,
    int? Visibility
);

public record WeatherRecordResponseDto(
    Guid Id,
    Guid CityId,
    string CityName,
    Guid WeatherTypeId,
    string WeatherTypeName,
    DateTime RecordedAt,
    double Temperature,
    double FeelsLike,
    int Humidity,
    double WindSpeed,
    int? WindDirection,
    int? Pressure,
    int? Visibility,
    DateTime CreatedAt
);

public record CurrentWeatherDto(
    string CityName,
    string Country,
    string WeatherType,
    string WeatherIcon,
    double Temperature,
    double FeelsLike,
    int Humidity,
    double WindSpeed,
    DateTime RecordedAt
);