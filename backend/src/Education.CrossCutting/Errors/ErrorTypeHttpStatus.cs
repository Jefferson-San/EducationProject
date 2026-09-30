using Education.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Education.CrossCutting.Errors;

public static class ErrorTypeHttpStatus
{
    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.External => StatusCodes.Status502BadGateway,
        _ => StatusCodes.Status500InternalServerError,
    };
}
