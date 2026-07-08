using AutoMapper;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.Story;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Tests.UnitTestSupport;

namespace SocialMediaPlatform.Tests.UnitTests;


public class StoryServiceTests
{
    private readonly Mock<IStoryRepository> _storyRepository = new(MockBehavior.Strict);
    private readonly Mock<INotificationRepository> _notificationRepository = new(MockBehavior.Strict);
    private readonly Mock<IFileStorageService> _fileStorageService = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly Mock<IPrivacyService> _privacyService = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWork = new(MockBehavior.Strict);

    private readonly StoryService _sut;

    public StoryServiceTests()
    {
        _sut = new StoryService(
            _storyRepository.Object,
            _notificationRepository.Object,
            _fileStorageService.Object,
            _mapper.Object,
            _privacyService.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task CreateStoryAsync_ShouldUploadMedia_PersistStory_AndReturnDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var file = TestHelpers.CreateFormFile();
        var mapped = new StoryDto();
        _fileStorageService.Setup(s => s.UploadFileAsync(file)).ReturnsAsync("https://cdn/story.jpg");
        _storyRepository.Setup(r => r.Add(It.IsAny<Story>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<StoryDto>(It.IsAny<Story>())).Returns(mapped);

        // Act
        var result = await _sut.CreateStoryAsync(userId, file);

        // Assert: story created from the uploaded URL, persisted, one commit.
        result.Should().BeSameAs(mapped);
        _storyRepository.Verify(r => r.Add(It.Is<Story>(s =>
            s.UserId == userId && s.MediaUrl == "https://cdn/story.jpg")), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserActiveStoriesAsync_ShouldEnforcePrivacy_ThenReturnMapped()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var entities = new List<Story> { new Story(targetUserId, "u1") };
        var dtos = new List<StoryDto> { new StoryDto() };
        _privacyService.Setup(p => p.EnsureCanAccessAsync(targetUserId, requesterId)).Returns(Task.CompletedTask);
        _storyRepository.Setup(r => r.GetUserActiveStoriesAsync(targetUserId, 1, 20)).ReturnsAsync(entities);
        _mapper.Setup(m => m.Map<List<StoryDto>>(entities)).Returns(dtos);

        // Act
        var result = await _sut.GetUserActiveStoriesAsync(targetUserId, requesterId);

        // Assert: privacy is enforced before returning the mapped list.
        result.Should().BeSameAs(dtos);
        _privacyService.Verify(p => p.EnsureCanAccessAsync(targetUserId, requesterId), Times.Once);
    }

    [Fact]
    public async Task AddStoryViewAsync_ShouldThrow_WhenStoryNotFound()
    {
        // Arrange
        var storyId = Guid.NewGuid();
        _storyRepository.Setup(r => r.GetStoryAsync(storyId)).ReturnsAsync((Story?)null);

        // Act + Assert
        var act = async () => await _sut.AddStoryViewAsync(storyId, Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Story not found.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddStoryViewAsync_ShouldBeIdempotent_WhenAlreadyViewed()
    {
        // Arrange: viewer already saw this story -> service returns early, nothing saved.
        var storyId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var story = new Story(Guid.NewGuid(), "url");
        _storyRepository.Setup(r => r.GetStoryAsync(storyId)).ReturnsAsync(story);
        _privacyService.Setup(p => p.EnsureCanAccessAsync(story.UserId, viewerId)).Returns(Task.CompletedTask);
        _storyRepository.Setup(r => r.HasUserViewedStoryAsync(storyId, viewerId)).ReturnsAsync(true);

        // Act
        await _sut.AddStoryViewAsync(storyId, viewerId);

        // Assert: no view added, no commit.
        _storyRepository.Verify(r => r.AddStoryView(It.IsAny<StoryView>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddStoryViewAsync_ShouldRecordView_AndSave_WhenNotYetViewed()
    {
        // Arrange
        var storyId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var story = new Story(Guid.NewGuid(), "url");
        _storyRepository.Setup(r => r.GetStoryAsync(storyId)).ReturnsAsync(story);
        _privacyService.Setup(p => p.EnsureCanAccessAsync(story.UserId, viewerId)).Returns(Task.CompletedTask);
        _storyRepository.Setup(r => r.HasUserViewedStoryAsync(storyId, viewerId)).ReturnsAsync(false);
        _storyRepository.Setup(r => r.AddStoryView(It.IsAny<StoryView>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.AddStoryViewAsync(storyId, viewerId);

        // Assert
        _storyRepository.Verify(r => r.AddStoryView(It.IsAny<StoryView>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteStoryAsync_ShouldThrow_WhenNotOwner()
    {
        // Arrange
        var storyId = Guid.NewGuid();
        var story = new Story(Guid.NewGuid(), "url"); // owned by someone else
        _storyRepository.Setup(r => r.GetStoryAsync(storyId)).ReturnsAsync(story);

        // Act + Assert
        var act = async () => await _sut.DeleteStoryAsync(storyId, Guid.NewGuid());
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only delete your own stories.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteStoryAsync_ShouldSoftDelete_AndSave_WhenOwner()
    {
        // Arrange
        var storyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var story = new Story(userId, "url");
        _storyRepository.Setup(r => r.GetStoryAsync(storyId)).ReturnsAsync(story);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.DeleteStoryAsync(storyId, userId);

        // Assert
        story.IsDeleted.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LikeStoryAsync_ShouldThrow_WhenAlreadyLiked()
    {
        // Arrange
        var storyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var story = new Story(Guid.NewGuid(), "url");
        _storyRepository.Setup(r => r.GetStoryAsync(storyId)).ReturnsAsync(story);
        _privacyService.Setup(p => p.EnsureCanAccessAsync(story.UserId, userId)).Returns(Task.CompletedTask);
        _storyRepository.Setup(r => r.GetStoryLikeAsync(storyId, userId)).ReturnsAsync(new StoryLike(userId, storyId));

        // Act + Assert
        var act = async () => await _sut.LikeStoryAsync(userId, storyId);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Already liked.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LikeStoryAsync_OnSomeoneElsesStory_ShouldAddLike_Notify_AndSave()
    {
        // Arrange: liking another user's story -> owner gets notified.
        var storyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var story = new Story(Guid.NewGuid(), "url"); // owner != userId
        _storyRepository.Setup(r => r.GetStoryAsync(storyId)).ReturnsAsync(story);
        _privacyService.Setup(p => p.EnsureCanAccessAsync(story.UserId, userId)).Returns(Task.CompletedTask);
        _storyRepository.Setup(r => r.GetStoryLikeAsync(storyId, userId)).ReturnsAsync((StoryLike?)null);
        _storyRepository.Setup(r => r.AddStoryLike(It.IsAny<StoryLike>()));
        _notificationRepository.Setup(r => r.Add(It.IsAny<Notification>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.LikeStoryAsync(userId, storyId);

        // Assert
        _storyRepository.Verify(r => r.AddStoryLike(It.IsAny<StoryLike>()), Times.Once);
        _notificationRepository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LikeStoryAsync_OnOwnStory_ShouldNotNotify_ButSave()
    {
        // Arrange: liking your OWN story -> no self-notification.
        var storyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var story = new Story(userId, "url"); // owner == userId
        _storyRepository.Setup(r => r.GetStoryAsync(storyId)).ReturnsAsync(story);
        _privacyService.Setup(p => p.EnsureCanAccessAsync(story.UserId, userId)).Returns(Task.CompletedTask);
        _storyRepository.Setup(r => r.GetStoryLikeAsync(storyId, userId)).ReturnsAsync((StoryLike?)null);
        _storyRepository.Setup(r => r.AddStoryLike(It.IsAny<StoryLike>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        // No notification setup on purpose: strict mock proves the branch is skipped.

        // Act
        await _sut.LikeStoryAsync(userId, storyId);

        // Assert
        _notificationRepository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnlikeStoryAsync_ShouldThrow_WhenLikeNotFound()
    {
        // Arrange
        var storyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _storyRepository.Setup(r => r.GetStoryLikeAsync(storyId, userId)).ReturnsAsync((StoryLike?)null);

        // Act + Assert
        var act = async () => await _sut.UnlikeStoryAsync(userId, storyId);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Like not found.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UnlikeStoryAsync_ShouldSoftDelete_AndSave()
    {
        // Arrange
        var storyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var like = new StoryLike(userId, storyId);
        _storyRepository.Setup(r => r.GetStoryLikeAsync(storyId, userId)).ReturnsAsync(like);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.UnlikeStoryAsync(userId, storyId);

        // Assert
        like.IsDeleted.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
