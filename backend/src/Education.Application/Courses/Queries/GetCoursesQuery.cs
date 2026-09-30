using Education.Domain.Common;
using MediatR;

namespace Education.Application.Courses.Queries;

/// <summary>MVP sem matrícula: todo usuário autenticado vê todos os cursos.</summary>
public sealed record GetCoursesQuery : IRequest<Result<IReadOnlyCollection<CourseSummaryDto>>>;

public sealed class GetCoursesQueryHandler
    : IRequestHandler<GetCoursesQuery, Result<IReadOnlyCollection<CourseSummaryDto>>>
{
    private readonly ICourseReadRepository _repository;

    public GetCoursesQueryHandler(ICourseReadRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyCollection<CourseSummaryDto>>> Handle(
        GetCoursesQuery request, CancellationToken cancellationToken) =>
        Result.Success(await _repository.GetAllAsync(cancellationToken));
}
