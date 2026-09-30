using Education.Application.Videos;
using Education.Domain.Common;
using Education.Domain.Interfaces.Videos;
using MediatR;

namespace Education.Application.Lessons.Queries;

public sealed record GetLessonByIdQuery(Guid LessonId) : IRequest<Result<LessonDto>>;

public sealed class GetLessonByIdQueryHandler : IRequestHandler<GetLessonByIdQuery, Result<LessonDto>>
{
    private readonly ILessonReadRepository _repository;
    private readonly IFileStorage _storage;

    public GetLessonByIdQueryHandler(ILessonReadRepository repository, IFileStorage storage)
    {
        _repository = repository;
        _storage = storage;
    }

    public async Task<Result<LessonDto>> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var lesson = await _repository.GetByIdAsync(request.LessonId, cancellationToken);
        if (lesson is null)
            return Result.Failure<LessonDto>(Error.NotFound("Lesson.NotFound", "Aula não encontrada."));

        return Result.Success(new LessonDto(
            lesson.Id, lesson.CourseId, lesson.Title, lesson.Description, lesson.Order,
            lesson.Video?.ToDto(_storage)));
    }
}
