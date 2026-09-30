using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using VideoProcessing.Contracts.Messaging;
using VideoProcessing.Processing;

namespace VideoProcessing.Messaging;

/// <summary>
/// Consome a fila video.uploaded, uma mensagem por vez (prefetch = 1) e com ack manual:
/// a mensagem só sai da fila depois de tratada.
/// </summary>
public sealed class VideoUploadedConsumer(
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<VideoUploadedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await ConnectWithRetryAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(Queues.VideoUploaded, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            VideoUploadedMessage? message;
            try
            {
                message = JsonSerializer.Deserialize<VideoUploadedMessage>(ea.Body.Span, JsonSerializerOptions.Web);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Mensagem inválida descartada ({DeliveryTag})", ea.DeliveryTag);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, stoppingToken);
                return;
            }

            if (message is null || message.VideoId == Guid.Empty || string.IsNullOrWhiteSpace(message.OriginalPath))
            {
                logger.LogError("Mensagem sem VideoId/OriginalPath descartada ({DeliveryTag})", ea.DeliveryTag);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, stoppingToken);
                return;
            }

            logger.LogInformation("VideoUploaded recebido: {VideoId}, redelivered={Redelivered}",
                message.VideoId, ea.Redelivered);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<VideoProcessor>();
                await processor.ProcessAsync(message, ea.Redelivered, stoppingToken);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Desligando: sem ack. Ao fechar a conexão, o RabbitMQ devolve a mensagem para a fila.
            }
            catch (Exception ex)
            {
                // Normalmente a API fora do ar. Tenta mais uma vez (reentrega); na segunda falha, descarta.
                var requeue = !ea.Redelivered;
                logger.LogError(ex, "Falha ao processar {VideoId}; requeue={Requeue}", message.VideoId, requeue);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(Queues.VideoUploaded, autoAck: false, consumer, stoppingToken);
        logger.LogInformation("Aguardando mensagens na fila {Queue}", Queues.VideoUploaded);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // encerramento normal
        }
    }

    private async Task<IConnection> ConnectWithRetryAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            ClientProvidedName = "video-processing"
        };

        while (true)
        {
            try
            {
                return await factory.CreateConnectionAsync(stoppingToken);
            }
            catch (BrokerUnreachableException) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning("RabbitMQ indisponível em {Host}:{Port}, nova tentativa em 5s", settings.HostName, settings.Port);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
