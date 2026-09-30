using FluentValidation;

namespace Education.Application.Courses.Commands;

public sealed class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .Length(3, 200).WithMessage("Título deve ter entre 3 e 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Descrição deve ter no máximo 4000 caracteres.");

        RuleFor(x => x.Order)
            .InclusiveBetween(1, 10_000).When(x => x.Order.HasValue)
            .WithMessage("Ordem deve estar entre 1 e 10000.");
    }
}
