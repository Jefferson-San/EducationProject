using Education.Domain.Common;
using Education.Domain.Entities.Auth;
using MediatR;

namespace Education.Application.Auth.Commands;

/// <param name="Role">Anulável para que a ausência no JSON seja um erro de validação, e não "Teacher" por padrão.</param>
public sealed record RegisterCommand(string Name, string Email, string Password, UserRole? Role)
    : IRequest<Result<AuthResponse>>;
