namespace Education.Domain.Common;

/// <summary>
/// Lançada pelo IUnitOfWork quando outro processo alterou o mesmo registro entre a leitura e a gravação
/// (concorrência otimista). Ex.: duas entregas da mesma mensagem tentando iniciar o processamento do vídeo.
/// </summary>
public sealed class ConcurrencyException(string message, Exception? innerException = null)
    : Exception(message, innerException);
