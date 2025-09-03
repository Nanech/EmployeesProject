namespace Employees.API.Models;

public class EmployeeDepartament
{
    public int Id { get; init; }
    public int EmployeeId { get; set; }
    public int DepartamentId { get; set; }
}