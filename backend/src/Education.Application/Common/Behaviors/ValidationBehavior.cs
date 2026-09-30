using Education.Domain.Common;
using FluentValidation;
using MediatR;

namespace Education.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) =>
        _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationErrors = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (validationErrors.Count == 0) return await next();

        var error = Error.Validation(
            "Validation.Failed",
            string.Join("; ", validationErrors.Select(e => e.ErrorMessage)));

        // Use reflection to create Result<TResponse> with the validation error
        return (TResponse)typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
            .MakeGenericMethod(typeof(TResponse).GenericTypeArguments.FirstOrDefault() ?? typeof(object))
            .Invoke(null, [error])!;
    }
}
