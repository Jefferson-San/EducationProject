using Education.Domain.Common;
using Education.Domain.Interfaces;
using Education.Domain.Interfaces.Videos;
using MediatR;

namespace Education.Application.Videos.Commands;

/// <summary>
/// Chamado pelo Worker antes de processar. Idempotência: a regra de transição fica no Video (StartProcessing)
/// e a concorrência otimista garante que duas entregas simultâneas da mesma mensagem não processem juntas.
/// </summary>
public sealed class StartVideoProcessingCommandHandler : IRequestHandler<StartVideoProcessingCommand, Result>
{
    private readonly IVideoRepository _videos;
    private readonly IUnitOfWork _unitOfWork;

    public StartVideoProcessingCommandHandler(IVideoRepository videos, IUnitOfWork unitOfWork)
    {
        _videos = videos;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(StartVideoProcessingCommand request, CancellationToken cancellationToken)
    {
        var video = await _videos.GetByIdAsync(request.VideoId, cancellationToken);
        if (video is null) return Result.Failure(VideoProcessingErrors.NotFound);

        var started = video.StartProcessing(request.Redelivered);
        if (started.IsFailure) return started;

        return await VideoProcessingErrors.SaveAsync(_unitOfWork, cancellationToken);
    }
}
