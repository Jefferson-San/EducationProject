using Education.Domain.Common;
using Education.Domain.Entities.Videos;
using Education.Domain.Interfaces;
using Education.Domain.Interfaces.Courses;
using Education.Domain.Interfaces.Videos;
using MediatR;

namespace Education.Application.Videos.Commands;

/// <summary>
/// 1. arquivo no Storage → 2. metadados no banco → 3. VideoUploadedEvent (publicado no SaveChanges)
/// → VideoUploadedEventHandler envia a mensagem para o Worker.
/// </summary>
public sealed class UploadVideoCommandHandler : IRequestHandler<UploadVideoCommand, Result<VideoDto>>
{
    private readonly ICourseRepository _courses;
    private readonly IVideoRepository _videos;
    private readonly IFileStorage _storage;
    private readonly IUnitOfWork _unitOfWork;

    public UploadVideoCommandHandler(
        ICourseRepository courses, IVideoRepository videos, IFileStorage storage, IUnitOfWork unitOfWork)
    {
        _courses = courses;
        _videos = videos;
        _storage = storage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<VideoDto>> Handle(UploadVideoCommand request, CancellationToken cancellationToken)
    {
        var course = await _courses.GetByLessonIdAsync(request.LessonId, cancellationToken);
        if (course is null)
            return Result.Failure<VideoDto>(Error.NotFound("Lesson.NotFound", "Aula não encontrada."));

        if (!course.IsOwnedBy(request.TeacherId))
            return Result.Failure<VideoDto>(Error.Forbidden(
                "Course.NotOwner", "Somente o professor dono do curso pode enviar vídeos."));

        var previous = await _videos.GetByLessonIdAsync(request.LessonId, cancellationToken);
        if (previous is { CanBeReplaced: false })
            return Result.Failure<VideoDto>(Error.Conflict(
                "Video.Processing", "O vídeo atual desta aula ainda está em processamento."));

        // Novo Id a cada upload: as URLs do vídeo antigo não se misturam com as do novo.
        var videoId = Guid.NewGuid();
        var originalPath = VideoStorageKeys.OriginalFile(videoId);
        await _storage.SaveAsync(originalPath, request.Content, cancellationToken);

        if (previous is not null) _videos.Remove(previous);
        var video = Video.Create(videoId, request.LessonId, originalPath);
        await _videos.AddAsync(video, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (previous is not null)
        {
            await _storage.DeleteAsync(VideoStorageKeys.OriginalFolder(previous.Id), cancellationToken);
            await _storage.DeleteAsync(VideoStorageKeys.ProcessedFolder(previous.Id), cancellationToken);
        }

        return Result.Success(video.ToDto(_storage));
    }
}
