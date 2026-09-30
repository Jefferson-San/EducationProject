using Education.Domain.Common;
using MediatR;

namespace Education.Application.Videos.Commands;

public sealed record CompleteVideoProcessingCommand(Guid VideoId, string StreamingPath, double? DurationSeconds)
    : IRequest<Result>;
