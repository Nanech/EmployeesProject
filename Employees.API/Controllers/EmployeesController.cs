using Employees.API.Models.DTOs;
using Employees.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Employees.API.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateEmployeeAsync(CreateEmployeeDto employee)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var result = await employeeService.AddEmployeeAsync(employee);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
                    
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEmployeeByIdAsync(int id)
    {
        var result = await employeeService.GetEmployeeByIdAsync(id);
        
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployeeAsync(int id)
    {
        var result = await employeeService.DeleteEmployeeAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return NoContent();
    }

    [HttpGet("by-company/{companyId:int}")]
    public async Task<IActionResult> GetEmployeesByCompanyIdAsync(int companyId)
    {
        var result = await employeeService.GetEmployeesByCompanyIdAsync(companyId);
 
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Data);
    }

    [HttpGet("by-department/{departmentId:int}")]
    public async Task<IActionResult> GetEmployeesByDepartmentIdAsync(int departmentId)
    {
        var result = await employeeService.GetEmployeesByDepartmentIdAsync(departmentId);
        
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Data);
    }
    
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchEmployeeAsync(int id, [FromBody] CreateEmployeeDto employee)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var result = await employeeService.PatchEmployeeAsync(id, employee);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
                    
        return NoContent();
    }
    
}