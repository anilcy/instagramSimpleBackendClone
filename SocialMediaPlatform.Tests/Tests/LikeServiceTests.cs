using AutoMapper;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Tests.Tests;

public class LikeServiceTests
{
    private readonly Mock<ILikeRepository> _likeRepository = new(MockBehavior.Strict);
    private readonly Mock<IPostRepository> _postRepository = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly LikeService _sut;

    public LikeServiceTests()
    {
        _sut = new LikeService(_likeRepository.Object, _postRepository.Object, _mapper.Object);
    }

    [Fact]
    public async Task ToggleLikeAsync_ShouldInsertLikeAndReturnTrue_WhenPostNotLiked()
    {
        var postId = 10;
        var userId = Guid.NewGuid();

        _likeRepository.Setup(r => r.IsPostLikedByUserAsync(postId, userId)).ReturnsAsync(false);
        _likeRepository.Setup(r => r.InsertAsync(It.IsAny<PostLike>())).Returns(Task.CompletedTask);
        _likeRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.ToggleLikeAsync(postId, userId);

        result.Should().BeTrue();
        _likeRepository.Verify(r => r.InsertAsync(It.Is<PostLike>(l =>
            l.PostId == postId &&
            l.UserId == userId &&
            l.IsDeleted == false)), Times.Once);
        _likeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ToggleLikeAsync_ShouldDeleteLikeAndReturnFalse_WhenPostAlreadyLiked()
    {
        var postId = 10;
        var userId = Guid.NewGuid();
        var existingLike = new PostLike { PostId = postId, UserId = userId };

        _likeRepository.Setup(r => r.IsPostLikedByUserAsync(postId, userId)).ReturnsAsync(true);
        _likeRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PostLike, bool>>>()))
            .ReturnsAsync(new[] { existingLike });
        _likeRepository.Setup(r => r.DeleteAsync(existingLike)).Returns(Task.CompletedTask);
        _likeRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.ToggleLikeAsync(postId, userId);

        result.Should().BeFalse();
        _likeRepository.Verify(r => r.DeleteAsync(existingLike), Times.Once);
        _likeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
