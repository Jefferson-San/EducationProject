using Education.Application.Lessons.Queries;
using Education.Domain.Common;
using Education.Domain.Interfaces;
using Education.Domain.Interfaces.Courses;
using MediatR;

namespace Education.Application.Courses.Commands;

public sealed class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, Result<LessonDto>>
{
    private readonly ICourseRepository _courses;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLessonCommandHandler(ICourseRepository courses, IUnitOfWork unitOfWork)
    {
        _courses = courses;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LessonDto>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        var course = await _courses.GetWithLessonsAsync(request.CourseId, cancellationToken);
        if (course is null)
            return Result.Failure<LessonDto>(Error.NotFound("Course.NotFound", "Curso não encontrado."));

        // A regra de dono do curso fica no agregado (Course.AddLesson).
        var added = course.AddLesson(request.TeacherId, request.Title, request.Description, request.Order);
        if (added.IsFailure) return Result.Failure<LessonDto>(added.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var lesson = added.Value;
        return Result.Success(new LessonDto(lesson.Id, lesson.CourseId, lesson.Title, lesson.Description, lesson.Order,
            Video: null));
    }
}
