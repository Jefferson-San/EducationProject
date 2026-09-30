using Education.Domain.Entities.Videos;

namespace Education.Application.Lessons.Queries;

/// <summary>Aula lida sem rastreamento, com o vídeo (se houver) para montar a URL de streaming.</summary>
public sealed record LessonReadModel(Guid Id, Guid CourseId, string Title, string? Description, int Order, Video? Video);

public interface ILessonReadRepository
{
    Task<LessonReadModel?> GetByIdAsync(Guid lessonId, CancellationToken cancellationToken = default);
}
