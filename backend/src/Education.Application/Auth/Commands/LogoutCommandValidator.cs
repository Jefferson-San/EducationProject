using FluentValidation;

namespace Education.Application.Auth.Commands;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token é obrigatório.");
    }
}
