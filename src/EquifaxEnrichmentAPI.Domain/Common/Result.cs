namespace EquifaxEnrichmentAPI.Domain.Common;

/// <summary>
/// Represents the result of an operation that may fail.
/// Follows functional programming pattern to avoid throwing exceptions for validation errors.
/// </summary>
/// <typeparam name="T">The type of the successful result value</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public string Error { get; }

    private Result(T value, bool isSuccess, string error)
    {
        if (isSuccess && value is null)
        {
            throw new InvalidOperationException("Success result must have a value");
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new InvalidOperationException("Failure result must have an error message");
        }

        IsSuccess = isSuccess;
        Value = value;
        Error = error ?? string.Empty;
    }

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    public static Result<T> Success(T value) =>
        new(value, true, string.Empty);

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    public static Result<T> Failure(string error) =>
        new(default!, false, error);
}
