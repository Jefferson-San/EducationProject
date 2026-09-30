namespace Education.Domain.Common;

public record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
    public static readonly Error NullValue = new("General.Null", "Null value was provided", ErrorType.Failure);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
    public static Error External(string code, string message) => new(code, message, ErrorType.External);
}

public enum ErrorType { None, Failure, Validation, NotFound, Conflict, Unauthorized, Forbidden, External }
