using Education.Domain.Entities.Videos;

namespace Education.Domain.Interfaces.Videos;

public interface IVideoRepository : IRepository<Video>
{
    Task<Video?> GetByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default);
}
