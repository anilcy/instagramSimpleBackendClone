using FluentAssertions;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace SocialMediaPlatform.Tests.Tests;

public class AuthServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManager = TestSupport.TestHelpers.CreateUserManagerMock();
    private readonly Mock<ITokenService> _tokenService = new(MockBehavior.Strict);
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userManager.Object, _tokenService.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailureWhenEmailAlreadyExists()
    {
        var request = new RegisterRequest
        {
            Email = "a@example.com",
            Username = "ani",
            FullName = "Anil Y",
            Password = "pass123!"
        };

        _userManager.Setup(u => u.FindByEmailAsync(request.Email)).ReturnsAsync(new AppUser { Email = request.Email });

        var result = await _sut.RegisterAsync(request);

        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        _userManager.Verify(u => u.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
        _tokenService.Verify(t => t.GenerateJwtToken(It.IsAny<AppUser>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailureWhenCreateFails()
    {
        var request = new RegisterRequest
        {
            Email = "a@example.com",
            Username = "ani",
            FullName = "Anil Y",
            Password = "pass123!"
        };

        _userManager.Setup(u => u.FindByEmailAsync(request.Email)).ReturnsAsync((AppUser?)null);
        _userManager.Setup(u => u.CreateAsync(It.IsAny<AppUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var result = await _sut.RegisterAsync(request);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Password too weak");
        _tokenService.Verify(t => t.GenerateJwtToken(It.IsAny<AppUser>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnTokenOnSuccess()
    {
        var request = new RegisterRequest
        {
            Email = "a@example.com",
            Username = "ani",
            FullName = "Anil Y",
            Password = "pass123!"
        };
        var token = "jwt-token";

        _userManager.Setup(u => u.FindByEmailAsync(request.Email)).ReturnsAsync((AppUser?)null);
        _userManager.Setup(u => u.CreateAsync(It.IsAny<AppUser>(), request.Password)).ReturnsAsync(IdentityResult.Success);
        _tokenService.Setup(t => t.GenerateJwtToken(It.IsAny<AppUser>())).Returns(token);

        var result = await _sut.RegisterAsync(request);

        result.Success.Should().BeTrue();
        result.Token.Should().Be(token);
        result.Errors.Should().BeEmpty();
        _tokenService.Verify(t => t.GenerateJwtToken(It.IsAny<AppUser>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailureWhenUserMissing()
    {
        var request = new LoginRequest
        {
            Email = "missing@example.com",
            Password = "pass123!"
        };

        _userManager.Setup(u => u.FindByEmailAsync(request.Email)).ReturnsAsync((AppUser?)null);

        var result = await _sut.LoginAsync(request);

        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        _tokenService.Verify(t => t.GenerateJwtToken(It.IsAny<AppUser>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailureWhenPasswordInvalid()
    {
        var request = new LoginRequest
        {
            Email = "a@example.com",
            Password = "wrong"
        };
        var user = new AppUser { Id = Guid.NewGuid(), Email = request.Email, UserName = "ani" };

        _userManager.Setup(u => u.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _userManager.Setup(u => u.CheckPasswordAsync(user, request.Password)).ReturnsAsync(false);

        var result = await _sut.LoginAsync(request);

        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        _userManager.Verify(u => u.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        _tokenService.Verify(t => t.GenerateJwtToken(It.IsAny<AppUser>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldUpdateLastLoginAndReturnUserProfileOnSuccess()
    {
        var request = new LoginRequest
        {
            Email = "a@example.com",
            Password = "pass123!"
        };
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = "ani",
            FullName = "Anil Y",
            ProfilePictureUrl = "https://img/profile.jpg"
        };
        var token = "jwt-token";
        var before = DateTime.UtcNow;

        _userManager.Setup(u => u.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _userManager.Setup(u => u.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
        _userManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _tokenService.Setup(t => t.GenerateJwtToken(user)).Returns(token);

        var result = await _sut.LoginAsync(request);

        result.Success.Should().BeTrue();
        result.Token.Should().Be(token);
        result.Id.Should().Be(user.Id);
        result.UserName.Should().Be(user.UserName);
        result.FullName.Should().Be(user.FullName);
        result.ProfilePictureUrl.Should().Be(user.ProfilePictureUrl);
        user.LastLoginDate.Should().BeAfter(before);
        _userManager.Verify(u => u.UpdateAsync(user), Times.Once);
        _tokenService.Verify(t => t.GenerateJwtToken(user), Times.Once);
    }
}

