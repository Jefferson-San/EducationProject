namespace Education.Domain.Interfaces;

/// <summary>
/// Publica mensagens de integração para outros serviços. Quem decide a fila de cada tipo
/// de mensagem é a implementação (Infrastructure), não quem publica.
/// </summary>
public interface IMessageBus
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;
}
