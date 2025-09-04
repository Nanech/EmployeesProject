using Employees.API.Models.DTOs;
using Employees.API.Repositories.Interfaces;
using Employees.API.Services.Interfaces;

namespace Employees.API.Services;

public class EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger) : IEmployeeService
{
   public async Task<ServiceResult<int>> AddEmployeeAsync(CreateEmployeeDto? employee)
    {
        try
        {
            // Валидация существования связанных сущностей
            if (!await ValidateCompanyAndDepartments(employee))
                return ServiceResult<int>.Failure("Invalid company or department IDs");

            // Вызов репозитория
            var employeeId = await employeeRepository.AddEmployeeAsync(employee);
            return employeeId == null ? ServiceResult<int>.Failure("Failed to add employee")
                : ServiceResult<int>.Success(employeeId.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while adding employee");
            return ServiceResult<int>.Failure("Failed to add employee");
        }
    }

    public async Task<ServiceResult<bool>> PatchEmployeeAsync(int employeeId, CreateEmployeeDto employee)
    {
        if (employeeId <= 0)
            return ServiceResult<bool>.Failure("Invalid employee ID");
        
        if (!await EmployeeExistsAsync(employeeId))
            return ServiceResult<bool>.Failure("Employee does not exist");
        
        if (employee.CompanyId > 0 && !await employeeRepository.CompanyExistsAsync(employee.CompanyId))
            return ServiceResult<bool>.Failure("Company does not exist");
        
        if (employee.DepartmentIds.Count != 0)
        {
            foreach (var departmentId in employee.DepartmentIds)
                if (!await employeeRepository.DepartmentExistsAsync(departmentId))
                    return ServiceResult<bool>.Failure("Department does not exist");
        }
        
        var result = await employeeRepository.PatchEmployeeAsync(employeeId, employee);
        return result ? ServiceResult<bool>.Success(true) : ServiceResult<bool>.Failure("Failed to update employee");
    }

    private async Task<bool> ValidateCompanyAndDepartments(CreateEmployeeDto employee)
    {
        if (employee.CompanyId <= 0) 
            return false;

        if (!await employeeRepository.CompanyExistsAsync(employee.CompanyId))
            return false;

        if (employee.DepartmentIds == null || !employee.DepartmentIds.Any())
            return false;

        foreach (var departmentId in employee.DepartmentIds)
        {
            if (departmentId <= 0 || !await employeeRepository.DepartmentExistsAsync(departmentId))
                return false;
        }

        return true;
    }

    public async Task<ServiceResult<EmployeeDto>> GetEmployeeByIdAsync(int employeeId)
    {
        if (employeeId <= 0) return ServiceResult<EmployeeDto>.Failure($"Employee ID cannot be {employeeId}");
        
        var exists = await EmployeeExistsAsync(employeeId);
        if (!exists) return ServiceResult<EmployeeDto>.Failure($"No employee found with ID {employeeId}");

        var employee = await employeeRepository.GetEmployeeByIdAsync(employeeId);
        return employee == null ? ServiceResult<EmployeeDto>.Failure("Error retrieving employee data") 
            : ServiceResult<EmployeeDto>.Success(employee);
    }

    public async Task<ServiceResult<bool>> DeleteEmployeeAsync(int employeeId)
    {
        var employeeExists = await employeeRepository.EmployeeExistsAsync(employeeId);
        if (!employeeExists) return ServiceResult<bool>.Failure("Employee does not exist");
        
        var result = await employeeRepository.DeleteEmployeeAsync(employeeId);
        return !result ? ServiceResult<bool>.Failure("Failed to delete employee") 
            : ServiceResult<bool>.Success(true);
    }

    public async Task<bool> EmployeeExistsAsync(int employeeId)
    {
        if (employeeId <= 0) return false;

        return await employeeRepository.EmployeeExistsAsync(employeeId);
    }

    public async Task<ServiceResult<IEnumerable<EmployeeDto>>> GetEmployeesByDepartmentIdAsync(int departmentId)
    {
        if (departmentId <= 0) return ServiceResult<IEnumerable<EmployeeDto>>.Failure("Invalid department ID");
        
        var exists = await employeeRepository.DepartmentExistsAsync(departmentId);
        if (!exists) return ServiceResult<IEnumerable<EmployeeDto>>.Failure("Department does not exist");

        var employees = await employeeRepository.GetEmployeesByDepartmentAsync(departmentId);
        return !employees.Any() ? ServiceResult<IEnumerable<EmployeeDto>>.Failure("No employees found for the specified department") 
            : ServiceResult<IEnumerable<EmployeeDto>>.Success(employees);
    }

    public async Task<ServiceResult<IEnumerable<EmployeeDto>>> GetEmployeesByCompanyIdAsync(int companyId)
    {
        if (companyId <= 0) return ServiceResult<IEnumerable<EmployeeDto>>.Failure($"Invalid company ID {companyId}");
        
        var exists = await employeeRepository.CompanyExistsAsync(companyId);
        if (!exists) return ServiceResult<IEnumerable<EmployeeDto>>.Failure($"No company found with ID {companyId}");
        
        var employees = await employeeRepository.GetEmployeesByCompanyAsync(companyId);
        return !employees.Any() ? 
            ServiceResult<IEnumerable<EmployeeDto>>.Failure($"No employees found for company with ID {companyId}") 
            : ServiceResult<IEnumerable<EmployeeDto>>.Success(employees);
    }
}