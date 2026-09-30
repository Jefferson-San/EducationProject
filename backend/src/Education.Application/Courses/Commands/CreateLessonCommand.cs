using Education.Application.Lessons.Queries;
using Education.Domain.Common;
using MediatR;

namespace Education.Application.Courses.Commands;

/// <param name="Order">Posição da aula; sem valor, vai para o final do curso.</param>
public sealed record CreateLessonCommand(Guid TeacherId, Guid CourseId, string Title, string? Description, int? Order)
    : IRequest<Result<LessonDto>>;
