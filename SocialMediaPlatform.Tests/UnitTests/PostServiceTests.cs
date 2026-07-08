using AutoMapper;
using Microsoft.AspNetCore.Http;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.PostDtos;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Tests.UnitTestSupport;

namespace SocialMediaPlatform.Tests.UnitTests;


public class PostServiceTests
{
    private readonly Mock<IPostRepository> _postRepository = new(MockBehavior.Strict);
    private readonly Mock<IFileStorageService> _fileStorageService = new(MockBehavior.Strict);
    private readonly Mock<IMediaRepository> _mediaRepository = new(MockBehavior.Strict);
    private readonly Mock<INotificationRepository> _notificationRepository = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly Mock<IPrivacyService> _privacyService = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWork = new(MockBehavior.Strict);

    private readonly PostService _sut;

    public PostServiceTests()
    {
        _sut = new PostService(
            _postRepository.Object,
            _fileStorageService.Object,
            _mediaRepository.Object,
            _notificationRepository.Object,
            _mapper.Object,
            _privacyService.Object,
            _unitOfWork.Object);
    }

    // ── CreatePostAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePostAsync_WithMedia_ShouldUploadEachFile_PersistPostAndMedia_AndReturnDto()
    {
        // Arrange: two media files attached.
        var userId = Guid.NewGuid();
        var file1 = TestHelpers.CreateFormFile("a.jpg");
        var file2 = TestHelpers.CreateFormFile("b.jpg");
        var dto = new PostCreateDto { Caption = "trip", MediaFiles = new List<IFormFile> { file1, file2 } };
        var mapped = new PostDto();

        _postRepository.Setup(r => r.Add(It.IsAny<Post>()));
        _fileStorageService.Setup(s => s.UploadFileAsync(It.IsAny<IFormFile>())).ReturnsAsync("https://cdn/x.jpg");
        _mediaRepository.Setup(r => r.Add(It.IsAny<Media>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<PostDto>(It.IsAny<Post>())).Returns(mapped);

        // Act
        var result = await _sut.CreatePostAsync(dto, userId);

        // Assert: one post, one upload + one Media per file, single commit.
        result.Should().BeSameAs(mapped);
        _postRepository.Verify(r => r.Add(It.Is<Post>(p => p.AuthorId == userId && p.Caption == "trip")), Times.Once);
        _fileStorageService.Verify(s => s.UploadFileAsync(It.IsAny<IFormFile>()), Times.Exactly(2));
        _mediaRepository.Verify(r => r.Add(It.IsAny<Media>()), Times.Exactly(2));
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePostAsync_WithoutMedia_ShouldPersistPost_WithNoMedia_AndReturnDto()
    {
        // Arrange: caption-only post (empty media list).
        var userId = Guid.NewGuid();
        var dto = new PostCreateDto { Caption = "text only", MediaFiles = new List<IFormFile>() };
        var mapped = new PostDto();

        _postRepository.Setup(r => r.Add(It.IsAny<Post>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<PostDto>(It.IsAny<Post>())).Returns(mapped);
        // No upload / media setups: strict mocks prove the loop body never runs.

        // Act
        var result = await _sut.CreatePostAsync(dto, userId);

        // Assert
        result.Should().BeSameAs(mapped);
        _fileStorageService.Verify(s => s.UploadFileAsync(It.IsAny<IFormFile>()), Times.Never);
        _mediaRepository.Verify(r => r.Add(It.IsAny<Media>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Reads ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPostsAsync_ShouldEnforcePrivacy_ThenReturnMapped()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var entities = new List<Post> { new Post(targetUserId, "c") };
        var dtos = new List<PostDto> { new PostDto() };
        _privacyService.Setup(p => p.EnsureCanAccessAsync(targetUserId, requesterId)).Returns(Task.CompletedTask);
        _postRepository.Setup(r => r.GetPostsByUserIdAsync(targetUserId, 1, 20)).ReturnsAsync(entities);
        _mapper.Setup(m => m.Map<List<PostDto>>(entities)).Returns(dtos);

        // Act
        var result = await _sut.GetPostsAsync(targetUserId, requesterId, 1, 20);

        // Assert
        result.Should().BeSameAs(dtos);
        _privacyService.Verify(p => p.EnsureCanAccessAsync(targetUserId, requesterId), Times.Once);
    }

    [Fact]
    public async Task GetPostByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        var postId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync((Post?)null);

        // Act + Assert
        var act = async () => await _sut.GetPostByIdAsync(postId);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Post not found.");
    }

    [Fact]
    public async Task GetPostByIdAsync_ShouldReturnMapped_WhenFound()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post(Guid.NewGuid(), "c");
        var mapped = new PostDto();
        _postRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync(post);
        _mapper.Setup(m => m.Map<PostDto>(post)).Returns(mapped);

        // Act
        var result = await _sut.GetPostByIdAsync(postId);

        // Assert
        result.Should().BeSameAs(mapped);
    }

    // ── UpdatePostAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task UpdatePostAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        var postId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync((Post?)null);

        // Act + Assert
        var act = async () => await _sut.UpdatePostAsync(postId, new PostUpdateDto { Caption = "x" }, Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Post not found.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePostAsync_ShouldThrow_WhenNotOwner()
    {
        // Arrange: post belongs to someone else.
        var postId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync(new Post(Guid.NewGuid(), "orig"));

        // Act + Assert
        var act = async () => await _sut.UpdatePostAsync(postId, new PostUpdateDto { Caption = "x" }, Guid.NewGuid());
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You can only edit your own posts.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePostAsync_ShouldUpdateCaption_AndSave_WhenOwner()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var post = new Post(userId, "old caption");
        _postRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync(post);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.UpdatePostAsync(postId, new PostUpdateDto { Caption = "new caption" }, userId);

        // Assert
        post.Caption.Should().Be("new caption");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── DeletePostAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task DeletePostAsync_ShouldThrow_WhenNotOwner()
    {
        // Arrange
        var postId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync(new Post(Guid.NewGuid(), "c"));

        // Act + Assert
        var act = async () => await _sut.DeletePostAsync(postId, Guid.NewGuid());
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You can only delete your own posts.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeletePostAsync_ShouldSoftDelete_AndSave_WhenOwner()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var post = new Post(userId, "c");
        _postRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync(post);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.DeletePostAsync(postId, userId);

        // Assert
        post.IsDeleted.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── LikePostAsync / UnlikePostAsync ──────────────────────────────────────

    [Fact]
    public async Task LikePostAsync_ShouldThrow_WhenPostNotFound()
    {
        // Arrange
        var postId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync((Post?)null);

        // Act + Assert
        var act = async () => await _sut.LikePostAsync(Guid.NewGuid(), postId);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Post not found.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LikePostAsync_ShouldThrow_WhenAlreadyLiked()
    {
        // Arrange: an existing like is found.
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync(new Post(Guid.NewGuid(), "c"));
        _postRepository.Setup(r => r.GetPostLikeAsync(userId, postId)).ReturnsAsync(new PostLike(userId, postId));

        // Act + Assert
        var act = async () => await _sut.LikePostAsync(userId, postId);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Already liked.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LikePostAsync_OnSomeoneElsesPost_ShouldAddLike_Notify_AndSave()
    {
        // Arrange: liking another user's post -> author notified.
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync(new Post(Guid.NewGuid(), "c")); // author != userId
        _postRepository.Setup(r => r.GetPostLikeAsync(userId, postId)).ReturnsAsync((PostLike?)null);
        _postRepository.Setup(r => r.AddPostLike(It.IsAny<PostLike>()));
        _notificationRepository.Setup(r => r.Add(It.IsAny<Notification>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.LikePostAsync(userId, postId);

        // Assert
        _postRepository.Verify(r => r.AddPostLike(It.IsAny<PostLike>()), Times.Once);
        _notificationRepository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LikePostAsync_OnOwnPost_ShouldNotNotify_ButSave()
    {
        // Arrange: liking your own post -> no self-notification.
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync(new Post(userId, "c")); // author == userId
        _postRepository.Setup(r => r.GetPostLikeAsync(userId, postId)).ReturnsAsync((PostLike?)null);
        _postRepository.Setup(r => r.AddPostLike(It.IsAny<PostLike>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        // No notification setup: strict mock proves it isn't called.

        // Act
        await _sut.LikePostAsync(userId, postId);

        // Assert
        _notificationRepository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnlikePostAsync_ShouldThrow_WhenLikeNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPostLikeAsync(userId, postId)).ReturnsAsync((PostLike?)null);

        // Act + Assert
        var act = async () => await _sut.UnlikePostAsync(userId, postId);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Like not found.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UnlikePostAsync_ShouldSoftDelete_AndSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var like = new PostLike(userId, postId);
        _postRepository.Setup(r => r.GetPostLikeAsync(userId, postId)).ReturnsAsync(like);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.UnlikePostAsync(userId, postId);

        // Assert
        like.IsDeleted.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
