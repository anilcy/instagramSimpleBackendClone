using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.UserDtos;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Tests.UnitTestSupport;

namespace SocialMediaPlatform.Tests.UnitTests;


public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
    private readonly Mock<IFollowRepository> _followRepository = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly Mock<UserManager<AppUser>> _userManager = TestHelpers.CreateUserManagerMock();
    private readonly Mock<IUnitOfWork> _unitOfWork = new(MockBehavior.Strict);

    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(
            _userRepository.Object,
            _followRepository.Object,
            _mapper.Object,
            _userManager.Object,
            _unitOfWork.Object);
    }

    private static AppUser MakeUser(Guid id) => new AppUser("user", "user@example.com", "User") { Id = id };

    [Fact]
    public async Task GetUserProfileAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((AppUser?)null);

        // Act
        var act = async () => await _sut.GetUserProfileAsync(userId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found");
    }

    [Fact]
    public async Task GetUserProfileAsync_ShouldPopulateCounts_WhenNoCurrentUser()
    {
        // Arrange: no currentUserId -> no relationship lookup, just the aggregate counts.
        var userId = Guid.NewGuid();
        var dto = new UserDto();
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(MakeUser(userId));
        _mapper.Setup(m => m.Map<UserDto>(It.IsAny<AppUser>())).Returns(dto);
        _userRepository.Setup(r => r.GetUserPostsCountAsync(userId)).ReturnsAsync(5);
        _followRepository.Setup(r => r.GetFollowersCountAsync(userId)).ReturnsAsync(10);
        _followRepository.Setup(r => r.GetFollowingCountAsync(userId)).ReturnsAsync(3);

        // Act
        var result = await _sut.GetUserProfileAsync(userId);

        // Assert: the service filled the DTO's counts and left FollowStatus unset.
        result.PostsCount.Should().Be(5);
        result.FollowersCount.Should().Be(10);
        result.FollowingCount.Should().Be(3);
        result.FollowStatus.Should().BeNull();
    }

    [Fact]
    public async Task GetUserProfileAsync_ShouldSetFollowStatus_WhenViewedByAnotherUser()
    {
        // Arrange: a different current user is viewing -> relationship is resolved.
        var userId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var dto = new UserDto();
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(MakeUser(userId));
        _mapper.Setup(m => m.Map<UserDto>(It.IsAny<AppUser>())).Returns(dto);
        _userRepository.Setup(r => r.GetUserPostsCountAsync(userId)).ReturnsAsync(0);
        _followRepository.Setup(r => r.GetFollowersCountAsync(userId)).ReturnsAsync(0);
        _followRepository.Setup(r => r.GetFollowingCountAsync(userId)).ReturnsAsync(0);
        _followRepository.Setup(r => r.GetFollowRelationshipAsync(currentUserId, userId))
            .ReturnsAsync(new Follow(currentUserId, userId, isPrivate: false)); // Accepted

        // Act
        var result = await _sut.GetUserProfileAsync(userId, currentUserId);

        // Assert
        result.FollowStatus.Should().Be(FollowStatus.Accepted);
    }

    [Fact]
    public async Task UpdateLastLoginAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((AppUser?)null);

        // Act + Assert
        var act = async () => await _sut.UpdateLastLoginAsync(userId);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetPrivacyAsync_ShouldFlipFlag_AndSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = MakeUser(userId); // starts public
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.SetPrivacyAsync(userId, true);

        // Assert
        user.IsPrivate.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAccountAsync_ShouldDeactivate_AndSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = MakeUser(userId); // starts active
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.DeactivateAccountAsync(userId);

        // Assert
        user.IsActive.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAccountAsync_ShouldSoftDelete_AndSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = MakeUser(userId);
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.SoftDeleteAccountAsync(userId);

        // Assert
        user.IsDeleted.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
