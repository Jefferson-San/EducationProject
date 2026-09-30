using Education.Domain.Entities.Videos;
using Education.Domain.Interfaces.Videos;
using Microsoft.EntityFrameworkCore;

namespace Education.Infrastructure.Persistence.Repositories;

public class VideoRepository : BaseRepository<Video>, IVideoRepository
{
    public VideoRepository(AppDbContext context) : base(context) { }

    public async Task<Video?> GetByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(x => x.LessonId == lessonId, cancellationToken);
}
