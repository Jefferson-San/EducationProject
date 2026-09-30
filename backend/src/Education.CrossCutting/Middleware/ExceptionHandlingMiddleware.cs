using Education.CrossCutting.Errors;
using Education.CrossCutting.Exceptions;
using Education.Domain.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Education.CrossCutting.Middleware;

/// <summary>
/// Ponto único de tratamento de exceções da API. Erros de negócio normalmente chegam como Result
/// (ResultExtensions.ToApiResult); aqui ficam as exceções: requisição malformada, arquivo grande demais,
/// conflito de concorrência e falhas inesperadas (banco, fila, bug), sempre no formato ApiErrorResponse.
/// </summary>
public class ExceptionHandlingMiddleware
{
    /// <summary>Convenção (nginx) para "cliente fechou a conexão antes da resposta".</summary>
    private const int ClientClosedRequest = 499;

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Cliente desistiu (fechou a aba, cancelou o upload): não há para quem responder.
            _logger.LogDebug("Request {Method} {Path} canceled by the client", context.Request.Method, context.Request.Path);
            if (!context.Response.HasStarted) context.Response.StatusCode = ClientClosedRequest;
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception occurred");
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, ex.Message, ex.Code);
        }
        catch (BadHttpRequestException ex) when (ex.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            _logger.LogWarning("Request body too large on {Path}", context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status413PayloadTooLarge,
                "O tamanho do envio excede o limite permitido (500 MB para vídeos).", "Request.TooLarge");
        }
        catch (BadHttpRequestException ex)
        {
            _logger.LogWarning(ex, "Bad request");
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest,
                "A requisição não pôde ser interpretada: " + ex.Message, "Request.Invalid");
        }
        catch (ConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict");
            await WriteErrorAsync(context, StatusCodes.Status409Conflict,
                "O registro foi alterado por outra operação. Tente novamente.", "Concurrency.Conflict");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            var message = _environment.IsDevelopment()
                ? $"{ex.GetType().Name}: {ex.Message}"
                : "Ocorreu um erro inesperado.";
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, message, "InternalServerError");
        }
    }

    private Task WriteErrorAsync(HttpContext context, int statusCode, string message, string code)
    {
        // Se o corpo já começou a ser enviado, não dá para trocar por uma resposta de erro.
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started; could not write error {Code}", code);
            return Task.CompletedTask;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new ApiErrorResponse(statusCode, message, code));
    }
}
