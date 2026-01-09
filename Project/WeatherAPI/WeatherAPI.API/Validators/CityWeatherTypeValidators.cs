using FluentValidation;
using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Validators;

public class CityWeatherTypeCreateValidator : AbstractValidator<CityWeatherTypeCreateDto>
{
    private static readonly string[] ValidSeasons = { "Winter", "Spring", "Summer", "Autumn", "All" };

    public CityWeatherTypeCreateValidator()
    {
        RuleFor(x => x.CityId)
            .NotEmpty().WithMessage("City ID is required");

        RuleFor(x => x.WeatherTypeId)
            .NotEmpty().WithMessage("Weather type ID is required");

        RuleFor(x => x.Frequency)
            .InclusiveBetween(0, 100).WithMessage("Frequency must be between 0 and 100%");

        RuleFor(x => x.Season)
            .Must(s => s == null || ValidSeasons.Contains(s))
            .WithMessage("Season must be one of: Winter, Spring, Summer, Autumn, All");
    }
}

public class CityWeatherTypeUpdateValidator : AbstractValidator<CityWeatherTypeUpdateDto>
{
    private static readonly string[] ValidSeasons = { "Winter", "Spring", "Summer", "Autumn", "All" };

    public CityWeatherTypeUpdateValidator()
    {
        RuleFor(x => x.Frequency)
            .InclusiveBetween(0, 100).WithMessage("Frequency must be between 0 and 100%");

        RuleFor(x => x.Season)
            .Must(s => s == null || ValidSeasons.Contains(s))
            .WithMessage("Season must be one of: Winter, Spring, Summer, Autumn, All");
    }
}