using Education.Application.Videos.Messages;
using Education.Domain.Events;
using Education.Domain.Interfaces;
using Education.Domain.Interfaces.Videos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Education.Application.Videos.Events;

/// <summary>
/// Reage ao evento de domínio VideoUploadedEvent enviando a mensagem de integração para o Worker.
/// Sem a mensagem o vídeo nunca seria processado, então uma falha aqui marca o vídeo como Failed.
/// </summary>
public sealed class VideoUploadedEventHandler : INotificationHandler<VideoUploadedEvent>
{
    private readonly IMessageBus _messageBus;
    private readonly IVideoRepository _videos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VideoUploadedEventHandler> _logger;

    public VideoUploadedEventHandler(
        IMessageBus messageBus, IVideoRepository videos, IUnitOfWork unitOfWork, ILogger<VideoUploadedEventHandler> logger)
    {
        _messageBus = messageBus;
        _videos = videos;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(VideoUploadedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await _messageBus.PublishAsync(
                new VideoUploadedMessage(notification.VideoId, notification.OriginalPath), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Falha ao enviar o vídeo {VideoId} para processamento", notification.VideoId);

            var video = await _videos.GetByIdAsync(notification.VideoId, CancellationToken.None);
            if (video?.FailToEnqueue("Não foi possível enviar o vídeo para processamento. Tente novamente.").IsSuccess == true)
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
        }
    }
}
