using Education.Domain.Common;
using Education.Domain.Entities.Auth;
using Education.Domain.Interfaces.Auth;
using MediatR;

namespace Education.Application.Auth.Commands;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    // Mesma mensagem para e-mail inexistente e senha errada: não revela quais e-mails estão cadastrados.
    private static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "E-mail ou senha inválidos.");

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AuthTokenIssuer _tokenIssuer;

    public LoginCommandHandler(IUserRepository users, IPasswordHasher passwordHasher, AuthTokenIssuer tokenIssuer)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenIssuer = tokenIssuer;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(User.NormalizeEmail(request.Email), cancellationToken);
        if (user is null) return Result.Failure<AuthResponse>(InvalidCredentials);

        var verification = _passwordHasher.Verify(user.PasswordHash, request.Password);
        if (verification == PasswordVerification.Failed) return Result.Failure<AuthResponse>(InvalidCredentials);

        if (verification == PasswordVerification.SuccessRehashNeeded)
            user.UpdatePasswordHash(_passwordHasher.Hash(request.Password));

        return Result.Success(await _tokenIssuer.IssueAsync(user, cancellationToken));
    }
}
