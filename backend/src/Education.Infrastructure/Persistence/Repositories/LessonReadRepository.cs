using Education.Application.Lessons.Queries;
using Microsoft.EntityFrameworkCore;

namespace Education.Infrastructure.Persistence.Repositories;

public class LessonReadRepository : ILessonReadRepository
{
    private readonly AppDbContext _context;

    public LessonReadRepository(AppDbContext context) => _context = context;

    public async Task<LessonReadModel?> GetByIdAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        var lesson = await _context.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == lessonId, cancellationToken);
        if (lesson is null) return null;

        var video = await _context.Videos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LessonId == lessonId, cancellationToken);

        return new LessonReadModel(lesson.Id, lesson.CourseId, lesson.Title, lesson.Description, lesson.Order, video);
    }
}
