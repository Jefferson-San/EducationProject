using Education.Domain.Entities.Courses;
using Education.Domain.Interfaces.Courses;
using Microsoft.EntityFrameworkCore;

namespace Education.Infrastructure.Persistence.Repositories;

public class CourseRepository : BaseRepository<Course>, ICourseRepository
{
    public CourseRepository(AppDbContext context) : base(context) { }

    public async Task<Course?> GetWithLessonsAsync(Guid courseId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(x => x.Lessons)
            .FirstOrDefaultAsync(x => x.Id == courseId, cancellationToken);

    public async Task<Course?> GetByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(x => x.Lessons.Any(l => l.Id == lessonId), cancellationToken);
}
