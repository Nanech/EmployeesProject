namespace Employees.API.Services;

public class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }

    private ServiceResult(bool isSuccess, T? data, string? error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public static ServiceResult<T> Success(T data)
        => new(true, data, null);

    public static ServiceResult<T> Failure(string error)
        => new(false, default, error);
}