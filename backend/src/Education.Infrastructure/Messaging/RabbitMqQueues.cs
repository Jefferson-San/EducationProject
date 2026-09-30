// Contrato duplicado em VideoProcessing/Contracts/Messaging: qualquer mudança aqui deve ser feita lá também.
using Education.Application.Videos.Messages;

namespace Education.Infrastructure.Messaging;

/// <summary>Fila de cada mensagem de integração. Detalhe de mensageria: a Application não conhece.</summary>
public static class RabbitMqQueues
{
    public const string VideoUploaded = "video.uploaded";

    private static readonly Dictionary<Type, string> QueueByMessage = new()
    {
        [typeof(VideoUploadedMessage)] = VideoUploaded
    };

    public static string For(Type messageType) =>
        QueueByMessage.TryGetValue(messageType, out var queue)
            ? queue
            : throw new InvalidOperationException($"Nenhuma fila configurada para a mensagem {messageType.Name}.");
}
