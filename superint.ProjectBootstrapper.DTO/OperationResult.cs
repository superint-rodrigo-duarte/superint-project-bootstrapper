namespace superint.ProjectBootstrapper.DTO;

public sealed class OperationResult
{
    public bool Success { get; private init; }
    public bool Skipped { get; private init; }
    public string Message { get; private init; } = string.Empty;
    public string? Details { get; private init; }
    public Exception? Exception { get; private init; }

    public static OperationResult Ok(string message, string? details = null) => new()
    {
        Success = true,
        Skipped = false,
        Message = message,
        Details = details
    };

    public static OperationResult Fail(string message, Exception? exception = null) => new()
    {
        Success = false,
        Skipped = false,
        Message = message,
        Exception = exception,
        Details = exception?.Message
    };

    public static OperationResult Skip(string message) => new()
    {
        Success = true,
        Skipped = true,
        Message = message
    };
}