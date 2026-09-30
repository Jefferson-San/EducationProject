using Education.Domain.Common;
using Education.Domain.Interfaces;

namespace Education.Application.Videos.Commands;

/// <summary>Trechos comuns aos comandos chamados pelo Worker (start/complete/fail).</summary>
internal static class VideoProcessingErrors
{
    public static readonly Error NotFound = Error.NotFound("Video.NotFound", "Vídeo não encontrado.");

    private static readonly Error ConcurrentChange = Error.Conflict(
        "Video.ConcurrentChange", "O vídeo foi alterado por outro processo ao mesmo tempo.");

    /// <summary>Grava a transição; se outro processo mudou o vídeo no meio tempo, responde Conflict (409).</summary>
    public static async Task<Result> SaveAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (ConcurrencyException)
        {
            return Result.Failure(ConcurrentChange);
        }
    }
}
