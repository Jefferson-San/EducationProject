// Contrato duplicado em Education.Infrastructure/Messaging/RabbitMqQueues.cs: qualquer mudança aqui deve ser feita lá também.
namespace VideoProcessing.Contracts.Messaging;

public static class Queues
{
    public const string VideoUploaded = "video.uploaded";
}
