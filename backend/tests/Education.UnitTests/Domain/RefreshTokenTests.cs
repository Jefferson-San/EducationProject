using Education.Domain.Entities.Auth;
using FluentAssertions;

namespace Education.UnitTests.Domain;

public class RefreshTokenTests
{
    [Fact]
    public void Revoke_Twice_ShouldFail_BecauseItSignalsTokenReuse()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "HASH", DateTime.UtcNow.AddDays(7));

        token.Revoke(DateTime.UtcNow).IsSuccess.Should().BeTrue();
        token.Revoke(DateTime.UtcNow).IsFailure.Should().BeTrue();
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ShouldCompareWithExpiresAt()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.Create(Guid.NewGuid(), "HASH", now.AddMinutes(1));

        token.IsExpired(now).Should().BeFalse();
        token.IsExpired(now.AddMinutes(2)).Should().BeTrue();
    }
}
