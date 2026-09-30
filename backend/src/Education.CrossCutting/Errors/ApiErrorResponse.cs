namespace Education.CrossCutting.Errors;

public sealed record ApiErrorResponse(int StatusCode, string Message, string Code);
