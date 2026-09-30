using System.Text.Json;
using Education.Domain.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Education.Infrastructure.Messaging;

/// <summary>
/// Mantém uma única conexão com o RabbitMQ (singleton) e abre um canal por publicação,
/// já que canais não devem ser compartilhados entre threads.
/// </summary>
public sealed class RabbitMqMessageBus : IMessageBus, IAsyncDisposable
{
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;

    public RabbitMqMessageBus(IOptions<RabbitMqOptions> options) => _options = options;

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var queue = RabbitMqQueues.For(typeof(TMessage));
        var connection = await GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        var properties = new BasicProperties { Persistent = true, ContentType = "application/json" };

        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue, mandatory: false,
            basicProperties: properties, body: body, cancellationToken: cancellationToken);
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true }) return _connection;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true }) return _connection;

            var settings = _options.Value;
            var factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password,
                ClientProvidedName = "education-api"
            };
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) await _connection.DisposeAsync();
        _connectionLock.Dispose();
    }
}
