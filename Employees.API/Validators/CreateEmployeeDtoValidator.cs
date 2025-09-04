using Employees.API.Models.DTOs;
using FluentValidation;

namespace Employees.API.Validators;
 
public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeDtoValidator()
    {
        RuleFor(e => e).
            NotNull().WithMessage("Employee cannot be null");
        
        RuleFor(e => e.Name)
            .NotEmpty().WithMessage("Employee name is required")
            .MaximumLength(100).WithMessage("Employee name cannot exceed 100 characters");
        
        RuleFor(e => e.Surname).
            NotEmpty().WithMessage("Employee surname is required")
            .MaximumLength(100).WithMessage("Employee surname cannot exceed 100 characters");
        
        RuleFor(e => e.Phone).
            NotEmpty().WithMessage("Employee phone is required")
            .MaximumLength(15).WithMessage("Employee phone cannot exceed 15 characters");
        
        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("CompanyId must be greater than 0");

        RuleFor(e => e.Passport)
            .NotNull().WithMessage("Employee passport is required")
            .SetValidator(new PassportDtoValidator());
        
        RuleFor(e => e.DepartmentIds)
            .Must(x => x == null || x.All(id => id > 0))
            .WithMessage("All Department IDs must be greater than 0")
            .Must(x => x == null || x.Distinct().Any())
            .WithMessage("Department IDs must be unique");
        
        
    }
    
}