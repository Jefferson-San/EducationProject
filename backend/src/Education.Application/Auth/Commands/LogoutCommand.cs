using Education.Domain.Common;
using MediatR;

namespace Education.Application.Auth.Commands;

public sealed record LogoutCommand(Guid UserId, string RefreshToken) : IRequest<Result>;
