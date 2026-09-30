using Education.Domain.Common;
using Education.Domain.Interfaces;
using Education.Domain.Interfaces.Videos;
using MediatR;

namespace Education.Application.Videos.Commands;

public sealed class CompleteVideoProcessingCommandHandler : IRequestHandler<CompleteVideoProcessingCommand, Result>
{
    private readonly IVideoRepository _videos;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteVideoProcessingCommandHandler(IVideoRepository videos, IUnitOfWork unitOfWork)
    {
        _videos = videos;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteVideoProcessingCommand request, CancellationToken cancellationToken)
    {
        var video = await _videos.GetByIdAsync(request.VideoId, cancellationToken);
        if (video is null) return Result.Failure(VideoProcessingErrors.NotFound);

        var duration = request.DurationSeconds is { } seconds ? TimeSpan.FromSeconds(seconds) : (TimeSpan?)null;
        var completed = video.Complete(request.StreamingPath, duration);
        if (completed.IsFailure) return completed;

        return await VideoProcessingErrors.SaveAsync(_unitOfWork, cancellationToken);
    }
}
