using Education.Domain.Common;
using MediatR;

namespace Education.Application.Courses.Queries;

public sealed record GetCourseByIdQuery(Guid CourseId) : IRequest<Result<CourseDetailDto>>;

public sealed class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, Result<CourseDetailDto>>
{
    private readonly ICourseReadRepository _repository;

    public GetCourseByIdQueryHandler(ICourseReadRepository repository) => _repository = repository;

    public async Task<Result<CourseDetailDto>> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        var course = await _repository.GetByIdAsync(request.CourseId, cancellationToken);

        return course is null
            ? Result.Failure<CourseDetailDto>(Error.NotFound("Course.NotFound", "Curso não encontrado."))
            : Result.Success(course);
    }
}
