using Dapper;
using Employees.API.Database;
using Employees.API.Models.DTOs;
using Employees.API.Repositories.Interfaces;

namespace Employees.API.Repositories;

public class EmployeeRepository(IDbConnectionFactory connection) : IEmployeeRepository
{
    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId)
    {
        using var dbConnection = await connection.CreateConnectionAsync();

        EmployeeDto? employeeDto = null;

        var sql = $"""
           SELECT 
               e.id as {nameof(EmployeeDto.Id)},
               e.name as {nameof(EmployeeDto.Name)},
               e.surname as {nameof(EmployeeDto.Surname)},
               e.phone as {nameof(EmployeeDto.Phone)},
               e.company_id as {nameof(EmployeeDto.CompanyId)},
               p.type as {nameof(EmployeeDto.Passport.Type)}, 
               p.number AS {nameof(EmployeeDto.Passport.Number)},
               d.name as {nameof(DepartamentDto.Name)},
               d.phone as {nameof(DepartamentDto.Phone)}
           FROM employees e
           LEFT JOIN passports p ON e.passport_id = p.id
           LEFT JOIN employee_departments de ON e.id = de.employee_id
           LEFT JOIN departments d ON de.department_id = d.id
           WHERE e.id = @Id
           """;
        await dbConnection.QueryAsync<EmployeeDto, PassportDto, DepartamentDto, EmployeeDto>(
            sql,
            (emp, passport, departmentDto) =>
            {
                if (employeeDto == null)
                {
                    employeeDto = emp;
                    employeeDto.Passport = passport;
                    employeeDto.Departments = new List<DepartamentDto>();
                }

                if (departmentDto != null && !string.IsNullOrEmpty(departmentDto.Name))
                {
                    employeeDto.Departments.Add(departmentDto);
                }

                return employeeDto;
            },
            new { Id = employeeId },
            splitOn: $"{nameof(EmployeeDto.Passport.Type)},{nameof(DepartamentDto.Name)}"
        );

        return employeeDto;
    }

    public async Task<int?> AddEmployeeAsync(CreateEmployeeDto employee)
    {
        using var dbConnection = await connection.CreateConnectionAsync();
        using var transaction = dbConnection.BeginTransaction();
        try
        {
            var passportSql = $"""
            INSERT INTO passports (type, number)
            VALUES (@{nameof(PassportDto.Type)}, @{nameof(PassportDto.Number)})
            RETURNING id;
            """;
            var passportId = await dbConnection.ExecuteScalarAsync<int>(passportSql, employee.Passport, transaction);

            const string employeeSql = $"""
            INSERT INTO employees
            (name, surname, phone, company_id, passport_id)
            VALUES
            (@{nameof(CreateEmployeeDto.Name)}, @{nameof(CreateEmployeeDto.Surname)}, @{nameof(CreateEmployeeDto.Phone)}, 
             @{nameof(CreateEmployeeDto.CompanyId)}, @PassportId)
            RETURNING id;
            """;
            var employeeId = await dbConnection.ExecuteScalarAsync<int>(employeeSql, new
            {
                employee.Name,
                employee.Surname,
                employee.Phone,
                employee.CompanyId,
                PassportId = passportId
            }, transaction);

            const string departmentEmployeeSql = """
            INSERT INTO employee_departments (department_id, employee_id)
            VALUES (@DepartmentId, @EmployeeId);
            """;
            foreach (var departmentId in employee.DepartmentIds)
            {
                await dbConnection.ExecuteAsync(departmentEmployeeSql,
                    new { DepartmentId = departmentId, EmployeeId = employeeId },
                    transaction);
            }

            transaction.Commit();
            return employeeId;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public async Task<bool> PatchEmployeeAsync(int employeeId, CreateEmployeeDto employee)
    {
        using var dbConnection = await connection.CreateConnectionAsync();
        using var transaction = dbConnection.BeginTransaction();
        try
        {
            var updateParams = new DynamicParameters();
            updateParams.Add("Id", employeeId);
            
            var updateFields = new List<string>();
            if (!string.IsNullOrEmpty(employee.Name))
            {
                updateFields.Add("name = @Name");
                updateParams.Add("Name", employee.Name);
            }
            
            if (!string.IsNullOrEmpty(employee.Surname))
            {
                updateFields.Add("surname = @Surname");
                updateParams.Add("Surname", employee.Surname);
            }

            if (!string.IsNullOrEmpty(employee.Phone))
            {
                updateFields.Add("phone = @Phone");
                updateParams.Add("Phone", employee.Phone);
            }

            if (employee.CompanyId > 0)
            {
                updateFields.Add("company_id = @CompanyId");
                updateParams.Add("CompanyId", employee.CompanyId);
            }

            if (employee.Passport != null)
            {
                var passportParams = new DynamicParameters();
                var passportSql = "SELECT passport_id FROM employees WHERE id = @Id";
                var passportId = await dbConnection.QuerySingleOrDefaultAsync<int>(passportSql, 
                    new { Id = employeeId }, transaction);
                
                var passportFields = new List<string>();
                passportParams.Add("Id", passportId);
                if (!string.IsNullOrEmpty(employee.Passport.Type))
                {
                    passportFields.Add("type == @Type");
                    passportParams.Add("Type", employee.Passport.Type);
                }

                if (!string.IsNullOrEmpty(employee.Passport.Number))
                {
                    passportFields.Add("number = @Number");
                    passportParams.Add("Number", employee.Passport.Number);
                }

                if (passportFields.Count > 0)
                {
                    var updatePassportSql = $"UPDATE passports SET {string.Join(", ", passportFields)} WHERE id = @Id";
                    await dbConnection.ExecuteAsync(updatePassportSql, passportParams, transaction);
                }
            }

            if (employee.DepartmentIds != null && employee.DepartmentIds.Count > 0)
            {
                await dbConnection.ExecuteAsync(
                    "DELETE FROM employee_departments WHERE employee_id = @Id",
                    new { Id = employeeId }, transaction);

                foreach (var departmentId in employee.DepartmentIds)
                {
                    await dbConnection.ExecuteAsync(
                        "INSERT INTO employee_departments (department_id, employee_id) VALUES (@DepartmentId, @EmployeeId)",
                        new { DepartmentId = departmentId, EmployeeId = employeeId }, transaction);
                }
                
            }
            
            if (updateFields.Count > 0)
            {
                var updateSql = $"UPDATE employees SET {string.Join(", ", updateFields)} WHERE id = @Id";
                await dbConnection.ExecuteAsync(updateSql, updateParams, transaction);
            }
            transaction.Commit();
            return true;
        }
        catch 
        {
            transaction.Rollback();
            return false;
        }
    }

    
    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        using var dbConnection = await connection.CreateConnectionAsync();
        using var transaction = dbConnection.BeginTransaction();
        try
        {
            var sql = "SELECT passport_id FROM employees WHERE id = @Id";
            var passportId = await dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id }, transaction);
            
            sql = "DELETE FROM passports WHERE id = @Id";
            await dbConnection.ExecuteAsync(sql, new { Id = passportId }, transaction);
            
            sql = "DELETE FROM employees WHERE id = @Id";
            await dbConnection.ExecuteAsync(sql, new { Id = id }, transaction);
            
            transaction.Commit();
            return true;
        }
        catch (Exception)
        {
            transaction.Rollback();
            return false;
        }
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeesByCompanyAsync(int companyId)
    {
        using var dbConnection = await connection.CreateConnectionAsync();
        
        var employeeDictionary = new Dictionary<int, EmployeeDto>();
        var sql = $"""
        SELECT 
            e.id as {nameof(EmployeeDto.Id)},
            e.name as {nameof(EmployeeDto.Name)},
            e.surname as {nameof(EmployeeDto.Surname)},
            e.phone as {nameof(EmployeeDto.Phone)},
            e.company_id as {nameof(EmployeeDto.CompanyId)},
            p.type as {nameof(EmployeeDto.Passport.Type)}, 
            p.number AS {nameof(EmployeeDto.Passport.Number)},
            d.name as {nameof(DepartamentDto.Name)},
            d.phone as {nameof(DepartamentDto.Phone)}
        FROM employees e
        JOIN passports p ON p.id = e.passport_id
        JOIN employee_departments de ON e.id = de.employee_id
        JOIN departments d ON de.department_id = d.id
        WHERE e.company_id = @CompanyId
        """;
        await dbConnection.QueryAsync<EmployeeDto, PassportDto, DepartamentDto, EmployeeDto>(
            sql,
            (emp, passport, departmentDto) =>
            {
                if (!employeeDictionary.TryGetValue(emp.Id, out var employeeEntry))
                {
                    employeeEntry = emp;
                    employeeEntry.Passport = passport;
                    employeeEntry.Departments = new List<DepartamentDto>();
                    employeeDictionary.Add(employeeEntry.Id, employeeEntry);
                }
            
                if (departmentDto != null && !string.IsNullOrEmpty(departmentDto.Name))
                {
                    employeeEntry.Departments.Add(departmentDto);
                }
            
                return employeeEntry;
            },
            new { CompanyId = companyId },
            splitOn: $"{nameof(EmployeeDto.Passport.Type)},{nameof(DepartamentDto.Name)}"
        );
        return employeeDictionary.Values;
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        using var dbConnection = await connection.CreateConnectionAsync();
        
        var employeeDictionary = new Dictionary<int, EmployeeDto>();
        var sql = $"""
        SELECT 
            e.id as {nameof(EmployeeDto.Id)},
            e.name as {nameof(EmployeeDto.Name)},
            e.surname as {nameof(EmployeeDto.Surname)},
            e.phone as {nameof(EmployeeDto.Phone)},
            e.company_id as {nameof(EmployeeDto.CompanyId)},
            p.type as {nameof(EmployeeDto.Passport.Type)}, 
            p.number AS {nameof(EmployeeDto.Passport.Number)},
            d.name as {nameof(DepartamentDto.Name)},
            d.phone as {nameof(DepartamentDto.Phone)}
        FROM employee_departments ed
        JOIN employees e ON ed.employee_id = e.id
        JOIN passports p ON p.id = e.passport_id    
        JOIN departments d on ed.department_id = d.id             
        WHERE ed.department_id = @DepartmentId
        """;
        await dbConnection.QueryAsync<EmployeeDto, PassportDto, DepartamentDto, EmployeeDto>(
            sql,
            (emp, passport, departmentDto) =>
            {
                if (!employeeDictionary.TryGetValue(emp.Id, out var employeeEntry))
                {
                    employeeEntry = emp;
                    employeeEntry.Passport = passport;
                    employeeEntry.Departments = new List<DepartamentDto>();
                    employeeDictionary.Add(employeeEntry.Id, employeeEntry);
                }
            
                if (departmentDto != null && !string.IsNullOrEmpty(departmentDto.Name))
                {
                    employeeEntry.Departments.Add(departmentDto);
                }
            
                return employeeEntry;
            },
            new { DepartmentId = departmentId },
            splitOn: $"{nameof(EmployeeDto.Passport.Type)},{nameof(DepartamentDto.Name)}"
        );
        return employeeDictionary.Values;
    }

    // Helper methods to check existence. Some of them can be moved to other repositories if needed.
    public async Task<bool> CompanyExistsAsync(int companyId)
    {
        using var dbConnection = await connection.CreateConnectionAsync();
        const string sql = "SELECT COUNT(*) FROM companies WHERE id = @Id";
        var count = await dbConnection.ExecuteScalarAsync<int>(sql, new { Id = companyId });
        return count > 0;
    }

    public async Task<bool> DepartmentExistsAsync(int departmentId)
    {
        using var dbConnection = await connection.CreateConnectionAsync();
        const string sql = "SELECT COUNT(*) FROM departments WHERE id = @Id";
        var count = await dbConnection.ExecuteScalarAsync<int>(sql, new { Id = departmentId });
        return count > 0;
    }

    public async Task<bool> DoesEmployeeHasPassport(int employeeId)
    {
        using var dbConnection = await connection.CreateConnectionAsync();
        const string sql = "SELECT passport_id FROM employees WHERE id = @Id";
        var passportId = await dbConnection.QuerySingleOrDefaultAsync<int?>(sql, new { Id = employeeId });
        return passportId.HasValue;
    }

    public async Task<bool> EmployeeExistsAsync(int employeeId)
    {
        using var dbConnection = await connection.CreateConnectionAsync();
        const string sql = "SELECT 1 FROM employees WHERE id = @Id";
        var result = await dbConnection.QuerySingleOrDefaultAsync<int?>(sql, new { Id = employeeId });
        return result.HasValue;
    }
    
    

}