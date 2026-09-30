using Education.Domain.Common;
using Education.Domain.Entities.Auth;
using Education.Domain.Interfaces.Auth;
using MediatR;

namespace Education.Application.Auth.Commands;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AuthTokenIssuer _tokenIssuer;

    public RegisterCommandHandler(IUserRepository users, IPasswordHasher passwordHasher, AuthTokenIssuer tokenIssuer)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenIssuer = tokenIssuer;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = User.NormalizeEmail(request.Email);

        if (await _users.ExistsByEmailAsync(email, cancellationToken))
        {
            return Result.Failure<AuthResponse>(Error.Conflict(
                "User.EmailAlreadyExists", "Já existe um usuário com este e-mail."));
        }

        var user = User.Create(request.Name, email, _passwordHasher.Hash(request.Password), request.Role!.Value);
        await _users.AddAsync(user, cancellationToken);

        return Result.Success(await _tokenIssuer.IssueAsync(user, cancellationToken));
    }
}
