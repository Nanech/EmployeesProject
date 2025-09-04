namespace Employees.API.Models.DTOs;

public record CreateEmployeeDto
{
    public required string Name { get; init; }
    public required string Surname { get; init; }
    public required string Phone { get; init; }
    public required int CompanyId { get; init; }
    public required PassportDto Passport { get; init; }
    public ICollection<int> DepartmentIds { get; init; }
}