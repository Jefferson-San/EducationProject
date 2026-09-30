using Education.Application.Courses.Queries;
using Education.Domain.Entities.Videos;
using Microsoft.EntityFrameworkCore;

namespace Education.Infrastructure.Persistence.Repositories;

/// <summary>Lado de leitura: projeções direto nos DTOs, sem rastreamento do EF.</summary>
public class CourseReadRepository : ICourseReadRepository
{
    private readonly AppDbContext _context;

    public CourseReadRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyCollection<CourseSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await (
            from course in _context.Courses.AsNoTracking()
            join teacher in _context.Users on course.TeacherId equals teacher.Id
            orderby course.CreatedAt descending
            select new CourseSummaryDto(
                course.Id,
                course.Title,
                course.Description,
                course.TeacherId,
                teacher.Name,
                course.Lessons.Count,
                course.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task<CourseDetailDto?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken = default) =>
        await (
            from course in _context.Courses.AsNoTracking()
            join teacher in _context.Users on course.TeacherId equals teacher.Id
            where course.Id == courseId
            select new CourseDetailDto(
                course.Id,
                course.Title,
                course.Description,
                course.TeacherId,
                teacher.Name,
                course.CreatedAt,
                course.Lessons
                    .OrderBy(l => l.Order)
                    .Select(l => new LessonSummaryDto(
                        l.Id,
                        l.Title,
                        l.Description,
                        l.Order,
                        _context.Videos
                            .Where(v => v.LessonId == l.Id)
                            .Select(v => (VideoStatus?)v.Status)
                            .FirstOrDefault()))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
}
