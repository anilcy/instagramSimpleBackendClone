using AutoMapper;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.FollowDtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Tests.UnitTests;


public class FollowServiceTests
{
    // Arrange (shared): the collaborators
    private readonly Mock<IFollowRepository> _followRepository = new(MockBehavior.Strict);
    private readonly Mock<INotificationRepository> _notificationRepository = new(MockBehavior.Strict);
    private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWork = new(MockBehavior.Strict);

    private readonly FollowService _sut;

    // xUnit creates a brand-new instance of this class for every test method, so the
    // constructor is per-test setup , each test starts with fresh, empty mocks.
    public FollowServiceTests()
    {
        _sut = new FollowService(
            _followRepository.Object,
            _notificationRepository.Object,
            _userRepository.Object,
            _mapper.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task FollowUserAsync_ShouldRejectSelfFollow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act: capture the call as a delegate so FluentAssertions can await + inspect the throw.
        var act = async () => await _sut.FollowUserAsync(userId, userId);

        // Assert: it throws, AND it short-circuits before touching any collaborator.
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Cannot follow yourself");
        _followRepository.Verify(r => r.Add(It.IsAny<Follow>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FollowUserAsync_ShouldRejectDuplicateRelationship()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        // A non-null relationship already exists -> service must reject.
        _followRepository
            .Setup(r => r.GetFollowRelationshipAsync(currentUserId, targetUserId))
            .ReturnsAsync(new Follow(currentUserId, targetUserId, isPrivate: false));

        // Act
        var act = async () => await _sut.FollowUserAsync(currentUserId, targetUserId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Follow relationship already exists");
        _followRepository.Verify(r => r.Add(It.IsAny<Follow>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FollowUserAsync_ShouldRejectWhenTargetUserMissing()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        _followRepository
            .Setup(r => r.GetFollowRelationshipAsync(currentUserId, targetUserId))
            .ReturnsAsync((Follow?)null);
        _userRepository
            .Setup(r => r.GetByIdAsync(targetUserId))
            .ReturnsAsync((AppUser?)null);

        // Act
        var act = async () => await _sut.FollowUserAsync(currentUserId, targetUserId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Target user not found");
        _followRepository.Verify(r => r.Add(It.IsAny<Follow>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FollowUserAsync_PublicUser_ShouldCreateAcceptedFollow_NotifyAndSave()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new AppUser("target", "target@example.com", "Target User") { Id = targetUserId };
        // AppUser.IsPrivate defaults to false -> this is a public account.

        var expected = new FollowDto
        {
            FollowerId = currentUserId,
            FollowedId = targetUserId,
            Status = FollowStatus.Accepted
        };

        _followRepository
            .Setup(r => r.GetFollowRelationshipAsync(currentUserId, targetUserId))
            .ReturnsAsync((Follow?)null);
        _userRepository
            .Setup(r => r.GetByIdAsync(targetUserId))
            .ReturnsAsync(targetUser);
        // Void methods on a strict mock still need a Setup, otherwise the call throws.
        _followRepository.Setup(r => r.Add(It.IsAny<Follow>()));
        _notificationRepository.Setup(r => r.Add(It.IsAny<Notification>()));
        // The save now goes through IUnitOfWork — the whole point of the refactor.
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<FollowDto>(It.IsAny<Follow>())).Returns(expected);

        // Act
        var result = await _sut.FollowUserAsync(currentUserId, targetUserId);

        // Assert: returned DTO is what the mapper produced...
        result.Should().BeEquivalentTo(expected);
        // ...and the service built the RIGHT Follow (public => immediately Accepted)...
        _followRepository.Verify(r => r.Add(It.Is<Follow>(f =>
            f.FollowerId == currentUserId &&
            f.FollowedId == targetUserId &&
            f.Status == FollowStatus.Accepted)), Times.Once);
        // ...added a notification, and committed exactly once.
        _notificationRepository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FollowUserAsync_PrivateUser_ShouldCreatePendingFollow_AndSave()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new AppUser("target", "target@example.com", "Target User") { Id = targetUserId };
        targetUser.SetPrivate(true); // private account -> follow must be Pending.

        var expected = new FollowDto
        {
            FollowerId = currentUserId,
            FollowedId = targetUserId,
            Status = FollowStatus.Pending
        };

        _followRepository
            .Setup(r => r.GetFollowRelationshipAsync(currentUserId, targetUserId))
            .ReturnsAsync((Follow?)null);
        _userRepository
            .Setup(r => r.GetByIdAsync(targetUserId))
            .ReturnsAsync(targetUser);
        _followRepository.Setup(r => r.Add(It.IsAny<Follow>()));
        _notificationRepository.Setup(r => r.Add(It.IsAny<Notification>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<FollowDto>(It.IsAny<Follow>())).Returns(expected);

        // Act
        var result = await _sut.FollowUserAsync(currentUserId, targetUserId);

        // Assert
        result.Status.Should().Be(FollowStatus.Pending);
        _followRepository.Verify(r => r.Add(It.Is<Follow>(f => f.Status == FollowStatus.Pending)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RespondToFollowRequestAsync_ShouldAcceptPendingRequest_AndSave()
    {
        // Arrange
        var currentUserId = Guid.NewGuid(); // the private account owner, responding
        var requesterId = Guid.NewGuid();   // the person who asked to follow
        // isPrivate:true => the Follow starts in Pending status.
        var pending = new Follow(requesterId, currentUserId, isPrivate: true);
        var expected = new FollowDto
        {
            FollowerId = requesterId,
            FollowedId = currentUserId,
            Status = FollowStatus.Accepted
        };

        // Note the argument order: the service looks up (requesterId, currentUserId).
        _followRepository
            .Setup(r => r.GetFollowRelationshipAsync(requesterId, currentUserId))
            .ReturnsAsync(pending);
        _notificationRepository.Setup(r => r.Add(It.IsAny<Notification>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<FollowDto>(pending)).Returns(expected);

        // Act
        var result = await _sut.RespondToFollowRequestAsync(currentUserId, requesterId, FollowStatus.Accepted);

        // Assert: the domain object mutated (AcceptRequest) and we committed once.
        result.Should().BeEquivalentTo(expected);
        pending.Status.Should().Be(FollowStatus.Accepted);
        pending.DecidedAt.Should().NotBeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnfollowUserAsync_ShouldReturnFalse_WhenNoRelationshipExists()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        _followRepository
            .Setup(r => r.GetFollowRelationshipAsync(currentUserId, targetUserId))
            .ReturnsAsync((Follow?)null);

        // Act
        var result = await _sut.UnfollowUserAsync(currentUserId, targetUserId);

        // Assert: nothing to delete -> false, and no commit happened.
        result.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UnfollowUserAsync_ShouldSoftDeleteAndSave_WhenRelationshipExists()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var follow = new Follow(currentUserId, targetUserId, isPrivate: false);

        _followRepository
            .Setup(r => r.GetFollowRelationshipAsync(currentUserId, targetUserId))
            .ReturnsAsync(follow);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.UnfollowUserAsync(currentUserId, targetUserId);

        // Assert
        result.Should().BeTrue();
        follow.IsDeleted.Should().BeTrue(); // soft-delete flips the flag rather than removing the row
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
