using Employees.API.Models;
using Employees.API.Models.DTOs;

namespace Employees.API.Repositories.Interfaces;

public interface IEmployeeRepository
{
    Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId);
    Task<int?> AddEmployeeAsync(CreateEmployeeDto employeeDto);
    Task<bool> PatchEmployeeAsync(int employeeId, CreateEmployeeDto employee);
    Task<bool> DeleteEmployeeAsync(int employeeId);

    Task<IEnumerable<EmployeeDto>> GetEmployeesByCompanyAsync(int companyId);
    Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(int departmentId);

    Task<bool> EmployeeExistsAsync(int userId);
    Task<bool> CompanyExistsAsync(int companyId);
    Task<bool> DepartmentExistsAsync(int departmentId);
    Task<bool> DoesEmployeeHasPassport(int employeeId);
}