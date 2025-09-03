using Dapper;
using Employees.API.Database;
using Employees.API.Models;
using Employees.API.Repositories.Interfaces;

namespace Employees.API.Repositories;

public class EmployeeRepository : IEmployeeRepository
{

    private readonly IDbConnectionFactory _dbConnection;
    
    public EmployeeRepository(IDbConnectionFactory connection)
    {
        _dbConnection = connection;
    }
    
    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        using var dbConnection = await _dbConnection.CreateConnectionAsync();
        const string sql = "SELECT * from employees";
        return await dbConnection.QueryAsync<Employee>(sql);
    }

    public async Task<Employee> GetEmployeeByIdAsync(int id)
    {
        using var dbConnection = await _dbConnection.CreateConnectionAsync();
        const string sql = $"SELECT * from employees WHERE id = @{nameof(Employee.Id)}";
        return await dbConnection.QuerySingleOrDefaultAsync<Employee>(sql, new { Id = id }) 
               ?? throw new KeyNotFoundException($"Employee with id {id} not found");
    }

    public async Task<int> AddEmployeeAsync(Employee employee)
    { 
        using var dbConnection = await _dbConnection.CreateConnectionAsync();
        using var transaction = dbConnection.BeginTransaction();
        try
        {
            const string passportSql = $"""
            INSERT INTO passports (type, number)
            VALUES (@{nameof(Passport.Type)}, @{nameof(Passport.Number)})
            RETURNING id;
            """;
            var passportId = await dbConnection.ExecuteScalarAsync<int>(passportSql, employee.Passport, transaction);
            
            
            
            const string sql = $"""
            INSERT INTO employees (name, surname, phone)
            VALUES 
            (@{nameof(Employee.Name)},  @{nameof(Employee.Surname)},  @{nameof(Employee.Phone)})
            RETURNING id;
            """;
            return await dbConnection
                .ExecuteScalarAsync<int>(sql, new { employee.Name, employee.Surname, employee.Phone });
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw;
        }
    }

    public Task<IEnumerable<Employee>> SearchEmployeesByCompanyIdAsync(int companyId)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        using var dbConnection = await _dbConnection.CreateConnectionAsync();
        const string sql = "DELETE FROM employees WHERE id = @Id";
        var deletedRows = await dbConnection.ExecuteAsync(sql, new { Id = id });
        
    }
}