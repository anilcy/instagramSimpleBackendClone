using System.Security.Claims;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Tests.UnitTests;


public class TokenServiceTests
{
    // Build a TokenService with known JWT settings in the environment.
    private static TokenService CreateService()
    {
        // HmacSha256 requires a key of at least 256 bits (32 bytes), gotta keep this long.
        Environment.SetEnvironmentVariable("JWT_KEY", "test-signing-key-that-is-definitely-long-enough-123456");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");
        Environment.SetEnvironmentVariable("JWT_EXPIRE_MINUTES", "60");
        return new TokenService();
    }

    [Fact]
    public void GenerateJwtToken_ThenValidateToken_ShouldRoundTrip()
    {
        // Arrange
        var sut = CreateService();
        var user = new AppUser("alice", "alice@example.com", "Alice") { Id = Guid.NewGuid() };

        // Act: produce a token, then validate it back into a principal.
        var token = sut.GenerateJwtToken(user);
        var principal = sut.ValidateToken(token);

        // Assert: token is a non-empty string and carries the user's id as a claim.
        token.Should().NotBeNullOrWhiteSpace();
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Value == user.Id.ToString());
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_ForGarbageToken()
    {
        // Arrange
        var sut = CreateService();

        // Act
        var principal = sut.ValidateToken("this.is.not.a.jwt");

        // Assert: invalid tokens are swallowed and reported as null (not an exception).
        principal.Should().BeNull();
    }
}
