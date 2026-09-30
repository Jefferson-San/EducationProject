using Education.Domain.Common;
using Education.Domain.Interfaces;
using Education.Domain.Interfaces.Videos;
using MediatR;

namespace Education.Application.Videos.Commands;

public sealed class FailVideoProcessingCommandHandler : IRequestHandler<FailVideoProcessingCommand, Result>
{
    private readonly IVideoRepository _videos;
    private readonly IUnitOfWork _unitOfWork;

    public FailVideoProcessingCommandHandler(IVideoRepository videos, IUnitOfWork unitOfWork)
    {
        _videos = videos;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(FailVideoProcessingCommand request, CancellationToken cancellationToken)
    {
        var video = await _videos.GetByIdAsync(request.VideoId, cancellationToken);
        if (video is null) return Result.Failure(VideoProcessingErrors.NotFound);

        var failed = video.Fail(request.Reason);
        if (failed.IsFailure) return failed;

        return await VideoProcessingErrors.SaveAsync(_unitOfWork, cancellationToken);
    }
}
