using Employees.API.Models.DTOs;

namespace Employees.API.Services.Interfaces;

public interface IEmployeeService
{
    Task<ServiceResult<EmployeeDto>> GetEmployeeByIdAsync(int employeeId);
    Task<ServiceResult<int>> AddEmployeeAsync(CreateEmployeeDto? employee);
    Task<ServiceResult<bool>> PatchEmployeeAsync(int employeeId, CreateEmployeeDto employee);
    
    Task<ServiceResult<bool>> DeleteEmployeeAsync(int employeeId);
    Task<bool> EmployeeExistsAsync(int employeeId);

    Task<ServiceResult<IEnumerable<EmployeeDto>>> GetEmployeesByDepartmentIdAsync(int departmentId);
    Task<ServiceResult<IEnumerable<EmployeeDto>>> GetEmployeesByCompanyIdAsync(int companyId);
}