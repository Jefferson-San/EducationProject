using Education.Domain.Common;
using MediatR;

namespace Education.Application.Videos.Commands;

/// <param name="Redelivered">A mensagem foi reentregue pela fila (o Worker anterior pode ter caído no meio).</param>
public sealed record StartVideoProcessingCommand(Guid VideoId, bool Redelivered) : IRequest<Result>;
