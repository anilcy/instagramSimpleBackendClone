using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Tests.UnitTests;


public class PrivacyServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
    private readonly Mock<IFollowRepository> _followRepository = new(MockBehavior.Strict);

    private readonly PrivacyService _sut;

    public PrivacyServiceTests()
    {
        _sut = new PrivacyService(_userRepository.Object, _followRepository.Object);
    }

    // Small helper: build a user and optionally make the account private.
    private static AppUser MakeUser(Guid id, bool isPrivate)
    {
        var user = new AppUser("user", "user@example.com", "User") { Id = id };
        if (isPrivate) user.SetPrivate(true);
        return user;
    }

    [Fact]
    public async Task EnsureCanAccessAsync_ShouldThrow_WhenTargetUserNotFound()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(targetUserId)).ReturnsAsync((AppUser?)null);

        // Act
        var act = async () => await _sut.EnsureCanAccessAsync(targetUserId, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found");
    }

    [Fact]
    public async Task EnsureCanAccessAsync_ShouldAllow_WhenTargetIsPublic()
    {
        // Arrange: public account -> anyone can access; follow repo never consulted.
        var targetUserId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(targetUserId)).ReturnsAsync(MakeUser(targetUserId, isPrivate: false));

        // Act
        var act = async () => await _sut.EnsureCanAccessAsync(targetUserId, Guid.NewGuid());

        // Assert: it completes without throwing.
        await act.Should().NotThrowAsync();
        _followRepository.Verify(r => r.IsFollowingAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task EnsureCanAccessAsync_ShouldAllow_WhenRequesterIsTheOwner()
    {
        // Arrange: private account, but the requester IS the owner -> always allowed.
        var targetUserId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(targetUserId)).ReturnsAsync(MakeUser(targetUserId, isPrivate: true));

        // Act: requesterId == targetUserId
        var act = async () => await _sut.EnsureCanAccessAsync(targetUserId, targetUserId);

        // Assert
        await act.Should().NotThrowAsync();
        _followRepository.Verify(r => r.IsFollowingAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task EnsureCanAccessAsync_ShouldThrow_WhenPrivateAndRequesterNotFollowing()
    {
        // Arrange: private account, requester is a stranger who does NOT follow.
        var targetUserId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(targetUserId)).ReturnsAsync(MakeUser(targetUserId, isPrivate: true));
        _followRepository.Setup(r => r.IsFollowingAsync(requesterId, targetUserId)).ReturnsAsync(false);

        // Act
        var act = async () => await _sut.EnsureCanAccessAsync(targetUserId, requesterId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User's content is private");
    }

    [Fact]
    public async Task EnsureCanAccessAsync_ShouldAllow_WhenPrivateAndRequesterIsFollowing()
    {
        // Arrange: private account, but requester follows -> allowed.
        var targetUserId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(targetUserId)).ReturnsAsync(MakeUser(targetUserId, isPrivate: true));
        _followRepository.Setup(r => r.IsFollowingAsync(requesterId, targetUserId)).ReturnsAsync(true);

        // Act
        var act = async () => await _sut.EnsureCanAccessAsync(targetUserId, requesterId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureCanAccessAsync_ShouldThrow_WhenPrivateAndRequesterIsAnonymous()
    {
        // Arrange: private account, requesterId is null (not logged in) -> cannot be following.
        var targetUserId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(targetUserId)).ReturnsAsync(MakeUser(targetUserId, isPrivate: true));

        // Act
        var act = async () => await _sut.EnsureCanAccessAsync(targetUserId, null);

        // Assert: the `requesterId.HasValue && ...` short-circuits, so no follow lookup happens.
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User's content is private");
        _followRepository.Verify(r => r.IsFollowingAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }
}
