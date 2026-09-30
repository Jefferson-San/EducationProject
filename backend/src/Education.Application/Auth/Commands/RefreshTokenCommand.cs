using Education.Domain.Common;
using MediatR;

namespace Education.Application.Auth.Commands;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponse>>;
