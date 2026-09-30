using Education.Application.Courses.Queries;
using Education.Domain.Common;
using Education.Domain.Entities.Courses;
using Education.Domain.Interfaces;
using Education.Domain.Interfaces.Courses;
using MediatR;

namespace Education.Application.Courses.Commands;

public sealed class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Result<CourseDetailDto>>
{
    private readonly ICourseRepository _courses;
    private readonly ICourseReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCourseCommandHandler(
        ICourseRepository courses, ICourseReadRepository readRepository, IUnitOfWork unitOfWork)
    {
        _courses = courses;
        _readRepository = readRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CourseDetailDto>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = Course.Create(request.TeacherId, request.Title, request.Description);

        await _courses.AddAsync(course, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Relê pelo lado de leitura para devolver o mesmo formato do GET (inclui o nome do professor).
        return Result.Success((await _readRepository.GetByIdAsync(course.Id, cancellationToken))!);
    }
}
