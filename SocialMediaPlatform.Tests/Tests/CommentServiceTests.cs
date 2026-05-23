using AutoMapper;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Tests.Tests;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepository = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _sut = new CommentService(_commentRepository.Object, _mapper.Object);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldInsertTopLevelCommentAndMapResult()
    {
        var userId = Guid.NewGuid();
        var dto = new CreateCommentDto
        {
            PostId = 10,
            Content = "Nice post!",
            ParentCommentId = null
        };
        var expected = new CommentDto { Id = 5, PostId = dto.PostId, Content = dto.Content };

        _commentRepository.Setup(r => r.InsertAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);
        _commentRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<CommentDto>(It.IsAny<Comment>())).Returns(expected);

        var result = await _sut.AddCommentAsync(dto, userId);

        result.Should().BeEquivalentTo(expected);
        _commentRepository.Verify(r => r.InsertAsync(It.Is<Comment>(c =>
            c.PostId == dto.PostId &&
            c.AuthorId == userId &&
            c.Content == dto.Content &&
            c.ParentCommentId == null)), Times.Once);
        _commentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mapper.Verify(m => m.Map<CommentDto>(It.IsAny<Comment>()), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldRejectInvalidParentComment()
    {
        var userId = Guid.NewGuid();
        var dto = new CreateCommentDto
        {
            PostId = 10,
            Content = "Reply",
            ParentCommentId = 99
        };
        var parent = new Comment { Id = 99, PostId = 11 };

        _commentRepository.Setup(r => r.GetByIdAsync(dto.ParentCommentId.Value)).ReturnsAsync(parent);

        var act = async () => await _sut.AddCommentAsync(dto, userId);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid parent comment");
        _commentRepository.Verify(r => r.InsertAsync(It.IsAny<Comment>()), Times.Never);
        _commentRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetCommentsByPostIdAsync_ShouldMapReturnedComments()
    {
        var postId = 10;
        var comments = new List<Comment>
        {
            new() { Id = 1, PostId = postId, Content = "A" },
            new() { Id = 2, PostId = postId, Content = "B" }
        };
        var expected = new List<CommentDto>
        {
            new() { Id = 1, PostId = postId, Content = "A" },
            new() { Id = 2, PostId = postId, Content = "B" }
        };

        _commentRepository.Setup(r => r.GetCommentsByPostIdAsync(postId)).ReturnsAsync(comments);
        _mapper.Setup(m => m.Map<List<CommentDto>>(comments)).Returns(expected);

        var result = await _sut.GetCommentsByPostIdAsync(postId);

        result.Should().BeEquivalentTo(expected);
        _commentRepository.Verify(r => r.GetCommentsByPostIdAsync(postId), Times.Once);
        _mapper.Verify(m => m.Map<List<CommentDto>>(comments), Times.Once);
    }
}
