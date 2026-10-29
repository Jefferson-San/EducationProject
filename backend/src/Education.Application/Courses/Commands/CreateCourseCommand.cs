using Education.Application.Courses.Queries;
using Education.Domain.Common;
using Education.Domain.Entities.Courses;
using MediatR;

namespace Education.Application.Courses.Commands;

public sealed record CreateCourseCommand(Guid TeacherId, string Title, string? Description, CourseCategory? Category)
    : IRequest<Result<CourseDetailDto>>;
