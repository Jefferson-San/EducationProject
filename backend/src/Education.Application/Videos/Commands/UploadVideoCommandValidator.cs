using FluentValidation;

namespace Education.Application.Videos.Commands;

public sealed class UploadVideoCommandValidator : AbstractValidator<UploadVideoCommand>
{
    public UploadVideoCommandValidator()
    {
        RuleFor(x => x.FileName)
            .Must(name => string.Equals(Path.GetExtension(name), ".mp4", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Apenas arquivos .mp4 são aceitos.");

        RuleFor(x => x.Length)
            .InclusiveBetween(1, UploadVideoCommand.MaxFileSizeBytes)
            .WithMessage("O arquivo deve ter entre 1 byte e 500 MB.");
    }
}
