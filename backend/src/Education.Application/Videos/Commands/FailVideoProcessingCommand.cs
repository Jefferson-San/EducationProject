using Education.Domain.Common;
using MediatR;

namespace Education.Application.Videos.Commands;

public sealed record FailVideoProcessingCommand(Guid VideoId, string Reason) : IRequest<Result>;
