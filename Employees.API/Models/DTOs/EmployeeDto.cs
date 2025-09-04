namespace Employees.API.Models.DTOs;

public record EmployeeDto
{
    public int  Id { get; init; }
    public string Name { get; set; }
    public string Surname { get; set; }    
    public string Phone { get; set; }
    public int CompanyId { get; set; }
    public PassportDto Passport { get; set; }
    public ICollection<DepartamentDto>  Departments { get; set; }
}