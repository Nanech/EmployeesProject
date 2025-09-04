namespace Employees.API.Models.DTOs;

public record DepartamentDto
{
    public required string Name { get; init; }    
    public required string Phone { get; init; }
}
