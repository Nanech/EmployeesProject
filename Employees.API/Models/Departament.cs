namespace Employees.API.Models;

public class Departament
{
    public int Id { get; init; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public int CompanyId { get; set; }
}