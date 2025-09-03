using Employees.API.Models;

namespace Employees.API.Repositories.Interfaces;

public interface IEmployeeRepository
{
    Task<int> AddEmployeeAsync(Employee employee);
    Task DeleteEmployeeAsync(int id);
    Task<IEnumerable<Employee>> SearchEmployeesByCompanyIdAsync(int companyId);
    
    Task UpdateEmployeeAsync(Employee employee);
    
    // maybe not needed
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<Employee> GetEmployeeByIdAsync(int id);
}