namespace Education.Application.Courses.Queries;

/// <summary>Lado de leitura (CQRS): consultas projetadas direto nos DTOs, sem carregar o agregado.</summary>
public interface ICourseReadRepository
{
    Task<IReadOnlyCollection<CourseSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<CourseDetailDto?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken = default);
}
