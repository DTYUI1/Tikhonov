using FluentValidation;
using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Validators;

public class WeatherRecordCreateValidator : AbstractValidator<WeatherRecordCreateDto>
{
    public WeatherRecordCreateValidator()
    {
        RuleFor(x => x.CityId)
            .NotEmpty().WithMessage("City ID is required");

        RuleFor(x => x.WeatherTypeId)
            .NotEmpty().WithMessage("Weather type ID is required");

        RuleFor(x => x.RecordedAt)
            .NotEmpty().WithMessage("Recorded date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .WithMessage("Recorded date cannot be in the future");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(-100, 60).WithMessage("Temperature must be between -100 and 60°C");

        RuleFor(x => x.FeelsLike)
            .InclusiveBetween(-100, 60).WithMessage("FeelsLike must be between -100 and 60°C");

        RuleFor(x => x.Humidity)
            .InclusiveBetween(0, 100).WithMessage("Humidity must be between 0 and 100%");

        RuleFor(x => x.WindSpeed)
            .GreaterThanOrEqualTo(0).WithMessage("Wind speed cannot be negative")
            .LessThan(200).WithMessage("Wind speed seems unrealistic");

        RuleFor(x => x.WindDirection)
            .InclusiveBetween(0, 360).WithMessage("Wind direction must be between 0 and 360°")
            .When(x => x.WindDirection.HasValue);

        RuleFor(x => x.Pressure)
            .InclusiveBetween(800, 1200).WithMessage("Pressure must be between 800 and 1200 hPa")
            .When(x => x.Pressure.HasValue);

        RuleFor(x => x.Visibility)
            .GreaterThanOrEqualTo(0).WithMessage("Visibility cannot be negative")
            .When(x => x.Visibility.HasValue);
    }
}

public class WeatherRecordUpdateValidator : AbstractValidator<WeatherRecordUpdateDto>
{
    public WeatherRecordUpdateValidator()
    {
        RuleFor(x => x.CityId)
            .NotEmpty().WithMessage("City ID is required");

        RuleFor(x => x.WeatherTypeId)
            .NotEmpty().WithMessage("Weather type ID is required");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(-100, 60).WithMessage("Temperature must be between -100 and 60°C");

        RuleFor(x => x.Humidity)
            .InclusiveBetween(0, 100).WithMessage("Humidity must be between 0 and 100%");

        RuleFor(x => x.WindSpeed)
            .GreaterThanOrEqualTo(0).WithMessage("Wind speed cannot be negative");
    }
}