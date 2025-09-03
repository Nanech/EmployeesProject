namespace Employees.API.Models;

public class Employee
{
    public int  Id { get; init; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Phone { get; set; }
    public int CompanyId { get; set; }
    public Passport Passport { get; set; }
    public ICollection<EmployeeDepartament> EmployeeDepartaments { get; init; }
    
}