using FluentValidation;
using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Validators;

public class CityCreateValidator : AbstractValidator<CityCreateDto>
{
    public CityCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("City name is required")
            .MaximumLength(100).WithMessage("City name must not exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.TimeZone)
            .MaximumLength(50).WithMessage("TimeZone must not exceed 50 characters")
            .When(x => x.TimeZone != null);
    }
}

public class CityUpdateValidator : AbstractValidator<CityUpdateDto>
{
    public CityUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("City name is required")
            .MaximumLength(100).WithMessage("City name must not exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");
    }
}