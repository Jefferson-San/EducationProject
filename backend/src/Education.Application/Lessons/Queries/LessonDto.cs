using Education.Application.Videos;

namespace Education.Application.Lessons.Queries;

/// <summary>Aula com o vídeo (GET /v1/lessons/{id}); também serve de polling do status de processamento.</summary>
public sealed record LessonDto(
    Guid Id,
    Guid CourseId,
    string Title,
    string? Description,
    int Order,
    VideoDto? Video);
