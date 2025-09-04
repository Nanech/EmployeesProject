using Employees.API.Models.DTOs;
using FluentValidation;

namespace Employees.API.Validators;

public class PassportDtoValidator : AbstractValidator<PassportDto>
{
    public PassportDtoValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Passport type is required")
            .MaximumLength(10).WithMessage("Passport type cannot exceed 10 characters");
        
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Passport number is required")
            .MaximumLength(20).WithMessage("Passport number cannot exceed 20 characters");
    }
}