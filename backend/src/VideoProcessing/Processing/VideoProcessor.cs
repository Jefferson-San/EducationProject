using System.Diagnostics;
using VideoProcessing.Api;
using VideoProcessing.Contracts.Messaging;
using VideoProcessing.Contracts.Storage;
using VideoProcessing.Storage;

namespace VideoProcessing.Processing;

/// <summary>
/// Orquestra o processamento de um vídeo:
/// start (API) → FFmpeg em pasta temporária → move para processed/ → complete (API), ou fail em caso de erro.
/// </summary>
public sealed class VideoProcessor(
    IEducationApiClient api,
    IFFmpegService ffmpeg,
    IFileStorage storage,
    ILogger<VideoProcessor> logger)
{
    public async Task ProcessAsync(VideoUploadedMessage message, bool redelivered, CancellationToken cancellationToken)
    {
        var videoId = message.VideoId;

        // Idempotência: a API só libera se o vídeo estiver Uploaded/Failed (ou Processing numa reentrega).
        var start = await api.StartAsync(videoId, redelivered, cancellationToken);
        if (start != StartProcessingResult.Started)
        {
            logger.LogInformation("Vídeo {VideoId} ignorado: {Result}", videoId, start);
            return;
        }

        // Gera numa pasta de trabalho fora de processed/: o Nginx nunca enxerga um HLS pela metade.
        var workKey = $"work/{videoId}";
        try
        {
            var inputPath = storage.GetLocalPath(message.OriginalPath);
            if (!File.Exists(inputPath))
            {
                await api.FailAsync(videoId, "Arquivo original não encontrado no Storage.", cancellationToken);
                return;
            }

            storage.DeleteFolder(workKey);
            var stopwatch = Stopwatch.StartNew();
            var result = await ffmpeg.ConvertToHlsAsync(inputPath, storage.GetLocalPath(workKey), cancellationToken);

            if (!result.Success)
            {
                storage.DeleteFolder(workKey);
                logger.LogWarning("Falha ao processar {VideoId}: {Error}", videoId, result.Error);
                await api.FailAsync(videoId, result.Error!, cancellationToken);
                return;
            }

            storage.MoveFolder(workKey, StorageKeys.ProcessedFolder(videoId));
            await api.CompleteAsync(videoId, StorageKeys.MasterPlaylist(videoId), result.Duration, cancellationToken);

            logger.LogInformation("Vídeo {VideoId} processado em {Elapsed:mm\\:ss} ({Renditions})",
                videoId, stopwatch.Elapsed, string.Join(", ", result.Renditions));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Worker desligando: limpa e deixa a mensagem voltar para a fila (será reentregue).
            storage.DeleteFolder(workKey);
            throw;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            // Erro local (disco, FFmpeg ausente...): registra a falha na API para o professor ver.
            // HttpRequestException (API fora do ar) sobe para o consumer decidir sobre a reentrega.
            storage.DeleteFolder(workKey);
            logger.LogError(ex, "Erro inesperado ao processar {VideoId}", videoId);
            await api.FailAsync(videoId, $"Erro inesperado no processamento: {ex.Message}", cancellationToken);
        }
    }
}
