namespace SmartTaskManagement.Application.Common;

/// <summary>
/// Outcome of an Application operation that can fail with human-readable errors.
/// Keeps the Application layer free of API/HTTP concerns — the API maps a
/// <see cref="Result"/> onto its own response envelope and status codes.
/// </summary>
public class Result
{
    public bool Succeeded { get; }
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Category of an expected failure, used by the API to select an HTTP status
    /// code. <c>null</c> on success or when a failure has no specific category
    /// (the API treats an uncategorized failure as a validation/bad-request).
    /// </summary>
    public ErrorType? ErrorType { get; }

    protected Result(bool succeeded, IReadOnlyList<string> errors, ErrorType? errorType)
    {
        Succeeded = succeeded;
        Errors = errors;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, Array.Empty<string>(), null);

    public static Result Failure(params string[] errors) => new(false, errors, null);

    public static Result Failure(IEnumerable<string> errors) => new(false, errors.ToArray(), null);

    public static Result Failure(ErrorType errorType, params string[] errors) => new(false, errors, errorType);

    public static Result Failure(ErrorType errorType, IEnumerable<string> errors) => new(false, errors.ToArray(), errorType);
}

/// <summary>
/// A <see cref="Result"/> that carries a value on success.
/// </summary>
public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool succeeded, T? value, IReadOnlyList<string> errors, ErrorType? errorType)
        : base(succeeded, errors, errorType)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>(), null);

    public static new Result<T> Failure(params string[] errors) => new(false, default, errors, null);

    public static new Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors.ToArray(), null);

    public static new Result<T> Failure(ErrorType errorType, params string[] errors) => new(false, default, errors, errorType);

    public static new Result<T> Failure(ErrorType errorType, IEnumerable<string> errors) => new(false, default, errors.ToArray(), errorType);
}
