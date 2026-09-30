using Education.Domain.Common;
using MediatR;

namespace Education.Application.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;
