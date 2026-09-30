using Education.Application.Auth;
using Education.Application.Auth.Commands;
using Education.Domain.Common;
using Education.Domain.Entities.Auth;
using Education.Domain.Interfaces;
using Education.Domain.Interfaces.Auth;
using FluentAssertions;
using NSubstitute;

namespace Education.UnitTests.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _tokens.HashRefreshToken(Arg.Any<string>()).Returns(call => $"HASH-{call.Arg<string>()}");
        _tokens.CreateAccessToken(Arg.Any<User>()).Returns(new AccessToken("access", DateTime.UtcNow.AddMinutes(15)));
        _tokens.CreateRefreshToken().Returns(new NewRefreshToken("new-refresh", "HASH-new-refresh", DateTime.UtcNow.AddDays(7)));

        var issuer = new AuthTokenIssuer(_tokens, _refreshTokens, _unitOfWork);
        _handler = new RefreshTokenCommandHandler(_refreshTokens, _users, _tokens, issuer);
    }

    [Fact]
    public async Task ValidToken_ShouldRevokeIt_AndIssueANewPair()
    {
        var user = User.Create("Ana", "ana@t.com", "pwd-hash", UserRole.Student);
        var stored = RefreshToken.Create(user.Id, "HASH-old", DateTime.UtcNow.AddDays(1));
        _refreshTokens.GetByHashAsync("HASH-old", Arg.Any<CancellationToken>()).Returns(stored);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new RefreshTokenCommand("old"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RefreshToken.Should().Be("new-refresh");
        stored.IsRevoked.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReusedToken_ShouldRevokeAllUserTokens_AndReturnUnauthorized()
    {
        var userId = Guid.NewGuid();
        var alreadyRotated = RefreshToken.Create(userId, "HASH-old", DateTime.UtcNow.AddDays(1));
        alreadyRotated.Revoke(DateTime.UtcNow);
        _refreshTokens.GetByHashAsync("HASH-old", Arg.Any<CancellationToken>()).Returns(alreadyRotated);

        var result = await _handler.Handle(new RefreshTokenCommand("old"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
        result.Error.Code.Should().Be("Auth.RefreshTokenReused");
        await _refreshTokens.Received(1).RevokeAllActiveAsync(userId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        _tokens.DidNotReceive().CreateRefreshToken();
    }

    [Fact]
    public async Task ExpiredToken_ShouldReturnUnauthorized_WithoutIssuingTokens()
    {
        var expired = RefreshToken.Create(Guid.NewGuid(), "HASH-old", DateTime.UtcNow.AddMinutes(-1));
        _refreshTokens.GetByHashAsync("HASH-old", Arg.Any<CancellationToken>()).Returns(expired);

        var result = await _handler.Handle(new RefreshTokenCommand("old"), CancellationToken.None);

        result.Error.Code.Should().Be("Auth.ExpiredRefreshToken");
        _tokens.DidNotReceive().CreateRefreshToken();
    }
}
