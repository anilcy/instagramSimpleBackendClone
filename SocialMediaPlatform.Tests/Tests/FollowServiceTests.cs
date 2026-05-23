using AutoMapper;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Models;
using Moq;

namespace SocialMediaPlatform.Tests.Tests;

public class FollowServiceTests
{
    private readonly Mock<IFollowRepository> _followRepository = new(MockBehavior.Strict);
    private readonly Mock<INotificationService> _notificationService = new(MockBehavior.Strict);
    private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly FollowService _sut;

    public FollowServiceTests()
    {
        _sut = new FollowService(
            _followRepository.Object,
            _notificationService.Object,
            _userRepository.Object,
            _mapper.Object);
    }

    [Fact]
    public async Task FollowUserAsync_ShouldRejectSelfFollow()
    {
        var userId = Guid.NewGuid();

        var act = async () => await _sut.FollowUserAsync(userId, userId);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Cannot follow yourself");
    }

    [Fact]
    public async Task FollowUserAsync_ShouldRejectDuplicateRelationship()
    {
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        _followRepository.Setup(r => r.GetFollowRelationshipAsync(currentUserId, targetUserId))
            .ReturnsAsync(new Follow { FollowerId = currentUserId, FollowedId = targetUserId });

        var act = async () => await _sut.FollowUserAsync(currentUserId, targetUserId);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Follow relationship already exists");
    }

    [Fact]
    public async Task FollowUserAsync_ShouldRejectWhenTargetUserMissing()
    {
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        _followRepository.Setup(r => r.GetFollowRelationshipAsync(currentUserId, targetUserId)).ReturnsAsync((Follow?)null);
        _userRepository.Setup(r => r.GetByIdAsync(targetUserId)).ReturnsAsync((AppUser?)null);

        var act = async () => await _sut.FollowUserAsync(currentUserId, targetUserId);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Target user not found");
        _followRepository.Verify(r => r.InsertAsync(It.IsAny<Follow>()), Times.Never);
    }

    [Fact]
    public async Task FollowUserAsync_ShouldAcceptPublicUser_AndNotify()
    {
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new AppUser { Id = targetUserId, IsPrivate = false };
        var expected = new FollowDto { FollowerId = currentUserId, FollowedId = targetUserId, Status = FollowStatus.Accepted };

        _followRepository.Setup(r => r.GetFollowRelationshipAsync(currentUserId, targetUserId)).ReturnsAsync((Follow?)null);
        _userRepository.Setup(r => r.GetByIdAsync(targetUserId)).ReturnsAsync(targetUser);
        _followRepository.Setup(r => r.InsertAsync(It.IsAny<Follow>())).Returns(Task.CompletedTask);
        _followRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _notificationService.Setup(n => n.CreateFollowNotificationAsync(currentUserId, targetUserId)).Returns(Task.CompletedTask);
        _mapper.Setup(m => m.Map<FollowDto>(It.IsAny<Follow>())).Returns(expected);

        var result = await _sut.FollowUserAsync(currentUserId, targetUserId);

        result.Should().BeEquivalentTo(expected);
        _followRepository.Verify(r => r.InsertAsync(It.Is<Follow>(f =>
            f.FollowerId == currentUserId &&
            f.FollowedId == targetUserId &&
            f.Status == FollowStatus.Accepted)), Times.Once);
        _followRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _notificationService.Verify(n => n.CreateFollowNotificationAsync(currentUserId, targetUserId), Times.Once);
    }

    [Fact]
    public async Task FollowUserAsync_ShouldCreatePendingRequestForPrivateUserWithoutNotification()
    {
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new AppUser { Id = targetUserId, IsPrivate = true };
        var expected = new FollowDto { FollowerId = currentUserId, FollowedId = targetUserId, Status = FollowStatus.Pending };

        _followRepository.Setup(r => r.GetFollowRelationshipAsync(currentUserId, targetUserId)).ReturnsAsync((Follow?)null);
        _userRepository.Setup(r => r.GetByIdAsync(targetUserId)).ReturnsAsync(targetUser);
        _followRepository.Setup(r => r.InsertAsync(It.IsAny<Follow>())).Returns(Task.CompletedTask);
        _followRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<FollowDto>(It.IsAny<Follow>())).Returns(expected);

        var result = await _sut.FollowUserAsync(currentUserId, targetUserId);

        result.Status.Should().Be(FollowStatus.Pending);
        _notificationService.Verify(n => n.CreateFollowNotificationAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        _followRepository.Verify(r => r.InsertAsync(It.Is<Follow>(f => f.Status == FollowStatus.Pending)), Times.Once);
    }

    [Fact]
    public async Task RespondToFollowRequestAsync_ShouldAcceptPendingRequest_AndNotify()
    {
        var currentUserId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var follow = new Follow
        {
            FollowerId = requesterId,
            FollowedId = currentUserId,
            Status = FollowStatus.Pending
        };
        var expected = new FollowDto { FollowerId = requesterId, FollowedId = currentUserId, Status = FollowStatus.Accepted };

        _followRepository.Setup(r => r.GetFollowRelationshipAsync(requesterId, currentUserId)).ReturnsAsync(follow);
        _followRepository.Setup(r => r.UpdateAsync(follow)).Returns(Task.CompletedTask);
        _followRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _notificationService.Setup(n => n.CreateFollowNotificationAsync(requesterId, currentUserId)).Returns(Task.CompletedTask);
        _mapper.Setup(m => m.Map<FollowDto>(follow)).Returns(expected);

        var result = await _sut.RespondToFollowRequestAsync(currentUserId, requesterId, FollowStatus.Accepted);

        result.Success.Should().BeTrue();
        result.Status.Should().Be(FollowStatus.Accepted);
        result.Message.Should().Be("Follow request accepted");
        result.Follow.Should().BeEquivalentTo(expected);
        follow.Status.Should().Be(FollowStatus.Accepted);
        follow.DecidedAt.Should().NotBeNull();
        _followRepository.Verify(r => r.UpdateAsync(follow), Times.Once);
        _followRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _notificationService.Verify(n => n.CreateFollowNotificationAsync(requesterId, currentUserId), Times.Once);
    }

    [Fact]
    public async Task UnfollowUserAsync_ShouldReturnFalseWhenNoRelationshipExists()
    {
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        _followRepository.Setup(r => r.GetFollowRelationshipAsync(currentUserId, targetUserId)).ReturnsAsync((Follow?)null);

        var result = await _sut.UnfollowUserAsync(currentUserId, targetUserId);

        result.Should().BeFalse();
        _followRepository.Verify(r => r.DeleteAsync(It.IsAny<Follow>()), Times.Never);
        _followRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
