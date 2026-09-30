using Education.Application.Courses.Queries;
using Education.Domain.Common;
using MediatR;

namespace Education.Application.Courses.Commands;

public sealed record CreateCourseCommand(Guid TeacherId, string Title, string? Description)
    : IRequest<Result<CourseDetailDto>>;
