using AutoMapper;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.CommentDtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Tests.UnitTests;


public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepository = new(MockBehavior.Strict);
    private readonly Mock<INotificationRepository> _notificationRepository = new(MockBehavior.Strict);
    private readonly Mock<IPostRepository> _postRepository = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWork = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);

    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _sut = new CommentService(
            _commentRepository.Object,
            _notificationRepository.Object,
            _postRepository.Object,
            _unitOfWork.Object,
            _mapper.Object);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldThrow_WhenPostNotFound()
    {
        // Arrange: the post the user wants to comment on does not exist.
        var userId = Guid.NewGuid();
        var dto = new CommentCreateDto { PostId = Guid.NewGuid(), Content = "hi" };
        _postRepository.Setup(r => r.GetByIdAsync(dto.PostId)).ReturnsAsync((Post?)null);

        // Act
        var act = async () => await _sut.AddCommentAsync(dto, userId);

        // Assert: rejected before anything is written.
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Post not found");
        _commentRepository.Verify(r => r.Add(It.IsAny<Comment>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldThrow_WhenParentCommentBelongsToAnotherPost()
    {
        // Arrange: this is a reply (ParentCommentId set). The post exists, but the parent
        // comment belongs to a DIFFERENT post -> invalid reply.
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var dto = new CommentCreateDto { PostId = postId, Content = "reply", ParentCommentId = parentId };

        _postRepository.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new Post(Guid.NewGuid(), "caption"));
        // Parent's PostId (a brand new Guid) != dto.PostId -> triggers the guard.
        _commentRepository.Setup(r => r.GetByIdAsync(parentId))
            .ReturnsAsync(new Comment(Guid.NewGuid(), Guid.NewGuid(), "parent"));

        // Act
        var act = async () => await _sut.AddCommentAsync(dto, userId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid parent comment");
        _commentRepository.Verify(r => r.Add(It.IsAny<Comment>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddCommentAsync_TopLevelComment_OnSomeoneElsesPost_ShouldAddNotificationAndSave()
    {
        // Arrange: commenter is NOT the post author -> the author should be notified.
        var userId = Guid.NewGuid();
        var postAuthorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var dto = new CommentCreateDto { PostId = postId, Content = "nice!" }; // no ParentCommentId
        var mapped = new CommentDto();

        _postRepository.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new Post(postAuthorId, "caption"));
        _commentRepository.Setup(r => r.Add(It.IsAny<Comment>()));
        _notificationRepository.Setup(r => r.Add(It.IsAny<Notification>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<CommentDto>(It.IsAny<Comment>())).Returns(mapped);

        // Act
        var result = await _sut.AddCommentAsync(dto, userId);

        // Assert: comment saved + author notified + committed once; returns the mapped DTO.
        result.Should().BeSameAs(mapped);
        _commentRepository.Verify(r => r.Add(It.Is<Comment>(c =>
            c.PostId == postId && c.AuthorId == userId)), Times.Once);
        _notificationRepository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_OnOwnPost_ShouldNotNotify_ButStillSave()
    {
        // Arrange: commenter IS the post author -> no self-notification.
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var dto = new CommentCreateDto { PostId = postId, Content = "my own note" };
        var mapped = new CommentDto();

        _postRepository.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new Post(userId, "caption")); // author == userId
        _commentRepository.Setup(r => r.Add(It.IsAny<Comment>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<CommentDto>(It.IsAny<Comment>())).Returns(mapped);
        // NOTE: we deliberately do NOT set up _notificationRepository.Add. With a strict mock,
        // if the service tried to notify, the test would fail proving the branch is skipped.

        // Act
        var result = await _sut.AddCommentAsync(dto, userId);

        // Assert
        result.Should().BeSameAs(mapped);
        _notificationRepository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Never);
        _commentRepository.Verify(r => r.Add(It.IsAny<Comment>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldThrow_WhenNotOwner()
    {
        // Arrange: the comment belongs to someone else.
        var callerId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        _commentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(new Comment(Guid.NewGuid(), Guid.NewGuid(), "not yours"));

        // Act
        var act = async () => await _sut.DeleteCommentAsync(commentId, callerId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You can only delete your own comments.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldSoftDeleteAndSave_WhenOwner()
    {
        // Arrange: caller owns the comment.
        var callerId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var comment = new Comment(Guid.NewGuid(), callerId, "mine");
        _commentRepository.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.DeleteCommentAsync(commentId, callerId);

        // Assert: soft-deleted (flag flipped) and committed once.
        comment.IsDeleted.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
