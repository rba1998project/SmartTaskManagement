namespace SmartTaskManagement.API.Common;

/// <summary>
/// Consistent envelope for API responses. Error responses use the non-generic
/// form; successful responses carrying a payload use <see cref="ApiResponse{T}"/>.
/// </summary>
public class ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string>? Errors { get; init; }

    public static ApiResponse Fail(string message, IReadOnlyList<string>? errors = null)
    { 
        return new ApiResponse { Success = false, Message = message, Errors = errors };
    }

    public static ApiResponse<T> Ok<T>(T data, string message = "Request successful")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data };
    }
}

/// <summary>
/// An extended response wrapper used when an endpoint needs to return a data payload.
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }
}
