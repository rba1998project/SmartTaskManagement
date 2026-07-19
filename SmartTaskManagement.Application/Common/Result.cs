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

    protected Result(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public static Result Success() => new(true, Array.Empty<string>());

    public static Result Failure(params string[] errors) => new(false, errors);

    public static Result Failure(IEnumerable<string> errors) => new(false, errors.ToArray());
}

/// <summary>
/// A <see cref="Result"/> that carries a value on success.
/// </summary>
public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool succeeded, T? value, IReadOnlyList<string> errors)
        : base(succeeded, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());

    public static new Result<T> Failure(params string[] errors) => new(false, default, errors);

    public static new Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors.ToArray());
}
