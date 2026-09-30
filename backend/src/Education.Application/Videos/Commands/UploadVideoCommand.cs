using Education.Domain.Common;
using MediatR;

namespace Education.Application.Videos.Commands;

/// <summary>Envia (ou substitui) o vídeo de uma aula. O processamento acontece de forma assíncrona.</summary>
public sealed record UploadVideoCommand(Guid TeacherId, Guid LessonId, string FileName, long Length, Stream Content)
    : IRequest<Result<VideoDto>>
{
    public const long MaxFileSizeBytes = 500L * 1024 * 1024; // 500 MB
}
