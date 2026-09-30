using FluentValidation;

namespace Education.Application.Videos.Commands;

public sealed class CompleteVideoProcessingCommandValidator : AbstractValidator<CompleteVideoProcessingCommand>
{
    public CompleteVideoProcessingCommandValidator()
    {
        // Só aceita o master.m3u8 dentro da pasta processada deste vídeo (impede apontar para outro arquivo).
        RuleFor(x => x.StreamingPath)
            .Must((command, path) => path == VideoStorageKeys.MasterPlaylist(command.VideoId))
            .WithMessage(command => $"StreamingPath deve ser '{VideoStorageKeys.MasterPlaylist(command.VideoId)}'.");

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0).When(x => x.DurationSeconds.HasValue)
            .WithMessage("Duração deve ser maior que zero.");
    }
}
