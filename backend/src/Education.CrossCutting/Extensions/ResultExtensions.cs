using Education.CrossCutting.Errors;
using Education.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Education.CrossCutting.Extensions;

public static class ResultExtensions
{
    public static IResult ToApiResult(this Result result)
    {
        if (result.IsSuccess) return Results.NoContent();

        var statusCode = result.Error.Type.ToStatusCode();
        var response = new ApiErrorResponse(statusCode, result.Error.Message, result.Error.Code);
        return Results.Json(response, statusCode: statusCode);
    }

    public static IResult ToApiResult<T>(this Result<T> result) =>
        result.IsSuccess
            ? Results.Ok(result.Value)
            : ((Result)result).ToApiResult();
}
