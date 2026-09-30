using Education.Domain.Common;
using Education.Domain.Interfaces;
using Education.Domain.Interfaces.Auth;
using MediatR;

namespace Education.Application.Auth.Commands;

/// <summary>Revoga o refresh token do próprio usuário. Idempotente: sempre responde sucesso.</summary>
public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenService _tokens;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokens, ITokenService tokens, IUnitOfWork unitOfWork)
    {
        _refreshTokens = refreshTokens;
        _tokens = tokens;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var stored = await _refreshTokens.GetByHashAsync(_tokens.HashRefreshToken(request.RefreshToken), cancellationToken);

        if (stored is not null && stored.UserId == request.UserId && stored.Revoke(DateTime.UtcNow).IsSuccess)
        {
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (ConcurrencyException)
            {
                // Já foi revogado em paralelo (ex.: refresh simultâneo): o objetivo do logout foi atingido.
            }
        }

        return Result.Success();
    }
}
