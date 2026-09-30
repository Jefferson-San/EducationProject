using FluentValidation;

namespace Education.Application.Auth.Commands;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .Length(2, 150).WithMessage("Nome deve ter entre 2 e 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(254).WithMessage("E-mail deve ter no máximo 254 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .Length(8, 128).WithMessage("Senha deve ter entre 8 e 128 caracteres.");

        RuleFor(x => x.Role)
            .NotNull().WithMessage("Perfil (role) é obrigatório: Teacher ou Student.")
            .IsInEnum().WithMessage("Perfil (role) inválido: use Teacher ou Student.");
    }
}
