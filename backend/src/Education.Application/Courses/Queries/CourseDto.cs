using Education.Domain.Entities.Videos;

namespace Education.Application.Courses.Queries;

/// <summary>Item da listagem de cursos (GET /v1/courses).</summary>
public sealed record CourseSummaryDto(
    Guid Id,
    string Title,
    string? Description,
    Guid TeacherId,
    string TeacherName,
    int LessonCount,
    DateTime CreatedAt);

/// <summary>Curso com as aulas em ordem (GET /v1/courses/{id}).</summary>
public sealed record CourseDetailDto(
    Guid Id,
    string Title,
    string? Description,
    Guid TeacherId,
    string TeacherName,
    DateTime CreatedAt,
    IReadOnlyCollection<LessonSummaryDto> Lessons);

/// <param name="VideoStatus">Permite ao frontend marcar "em processamento" na lista sem abrir a aula.</param>
public sealed record LessonSummaryDto(
    Guid Id,
    string Title,
    string? Description,
    int Order,
    VideoStatus? VideoStatus);
