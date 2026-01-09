using FluentValidation;
using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Validators;

public class WeatherTypeCreateValidator : AbstractValidator<WeatherTypeCreateDto>
{
    public WeatherTypeCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Weather type name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters");

        RuleFor(x => x.IconCode)
            .NotEmpty().WithMessage("Icon code is required")
            .MaximumLength(10).WithMessage("Icon code must not exceed 10 characters");
    }
}

public class WeatherTypeUpdateValidator : AbstractValidator<WeatherTypeUpdateDto>
{
    public WeatherTypeUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Weather type name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters");

        RuleFor(x => x.IconCode)
            .NotEmpty().WithMessage("Icon code is required")
            .MaximumLength(10).WithMessage("Icon code must not exceed 10 characters");
    }
}