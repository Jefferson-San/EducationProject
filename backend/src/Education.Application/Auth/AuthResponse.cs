using Education.Domain.Entities.Auth;

namespace Education.Application.Auth;

public sealed record UserDto(Guid Id, string Name, string Email, UserRole Role);

public sealed record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserDto User);

internal static class UserMappings
{
    public static UserDto ToDto(this User user) => new(user.Id, user.Name, user.Email, user.Role);
}
