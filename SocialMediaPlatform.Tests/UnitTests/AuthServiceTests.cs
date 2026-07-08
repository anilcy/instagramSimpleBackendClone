using Microsoft.AspNetCore.Identity;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Entities.Dtos.AuthDtos;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Tests.UnitTestSupport;

namespace SocialMediaPlatform.Tests.UnitTests;


public class AuthServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManager = TestHelpers.CreateUserManagerMock();
    private readonly Mock<ITokenService> _tokenService = new(MockBehavior.Strict);

    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userManager.Object, _tokenService.Object);
    }

    // ── RegisterAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenEmailAlreadyExists()
    {
        // Arrange: an account with this email already exists.
        var request = new RegisterRequest { Email = "taken@example.com", Username = "new", Password = "P@ss1!" };
        _userManager.Setup(m => m.FindByEmailAsync(request.Email))
            .ReturnsAsync(new AppUser("someone", request.Email, null));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert: generic error message (does not leak whether it was email or password).
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Invalid email or password.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenUsernameTaken()
    {
        // Arrange: email is free, but the username is taken.
        var request = new RegisterRequest { Email = "free@example.com", Username = "taken", Password = "P@ss1!" };
        _userManager.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((AppUser?)null);
        _userManager.Setup(m => m.FindByNameAsync(request.Username))
            .ReturnsAsync(new AppUser(request.Username, "other@example.com", null));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("This username is already taken.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_AndSurfaceIdentityErrors_WhenCreateFails()
    {
        // Arrange: email + username free, but Identity rejects the password.
        var request = new RegisterRequest { Email = "free@example.com", Username = "new", Password = "weak" };
        _userManager.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((AppUser?)null);
        _userManager.Setup(m => m.FindByNameAsync(request.Username)).ReturnsAsync((AppUser?)null);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert: the Identity error descriptions are passed through to the caller.
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Password too weak");
    }

    [Fact]
    public async Task RegisterAsync_ShouldSucceed_AndReturnToken()
    {
        // Arrange: everything valid; Identity creates the user; token is generated.
        var request = new RegisterRequest { Email = "new@example.com", Username = "new", FullName = "New User", Password = "P@ss1!" };
        _userManager.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((AppUser?)null);
        _userManager.Setup(m => m.FindByNameAsync(request.Username)).ReturnsAsync((AppUser?)null);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), request.Password)).ReturnsAsync(IdentityResult.Success);
        _tokenService.Setup(t => t.GenerateJwtToken(It.IsAny<AppUser>())).Returns("jwt-token");

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Token.Should().Be("jwt-token");
        result.UserName.Should().Be(request.Username);
    }

    // ── LoginAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenUserNotFound()
    {
        // Arrange
        var request = new LoginRequest { Email = "ghost@example.com", Password = "whatever" };
        _userManager.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((AppUser?)null);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("User not found.");
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenPasswordInvalid()
    {
        // Arrange: user exists but password check fails.
        var request = new LoginRequest { Email = "user@example.com", Password = "wrong" };
        var user = new AppUser("user", request.Email, null);
        _userManager.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, request.Password)).ReturnsAsync(false);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_ShouldSucceed_UpdateLastLogin_AndReturnToken()
    {
        // Arrange: valid credentials.
        var request = new LoginRequest { Email = "user@example.com", Password = "correct" };
        var user = new AppUser("user", request.Email, "User");
        _userManager.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _tokenService.Setup(t => t.GenerateJwtToken(user)).Returns("jwt-token");

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert: success + token, and the user's last-login was persisted.
        result.Success.Should().BeTrue();
        result.Token.Should().Be("jwt-token");
        _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }
}
