using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Data.Repositories;
using SocialMediaPlatform.Entities.Dtos.PostDtos;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Tests.IntegrationSupport;
using SocialMediaPlatform.Tests.UnitTestSupport;

namespace SocialMediaPlatform.Tests.IntegrationTests;


public class PostServiceIntegrationTests : IntegrationTestBase
{
    // Wire a real PostService on a given context. Every dependency is real except the
    // file storage (an external system), which we pass in as a mock.
    private PostService CreateSut(SocialMediaDbContext ctx, IFileStorageService fileStorage)
    {
        var privacy = new PrivacyService(new UserRepository(ctx), new FollowRepository(ctx));
        return new PostService(
            new PostRepository(ctx),
            fileStorage,
            new MediaRepository(ctx),
            new NotificationRepository(ctx),
            Mapper,
            privacy,
            new UnitOfWork(ctx));
    }

    private static Mock<IFileStorageService> FileStorageReturning(string url)
    {
        var mock = new Mock<IFileStorageService>();
        mock.Setup(s => s.UploadFileAsync(It.IsAny<IFormFile>())).ReturnsAsync(url);
        return mock;
    }

    // Insert a post row directly (arrange step for the delete/like scenarios).
    private async Task<Guid> SeedPostAsync(Guid authorId, string caption)
    {
        var post = new Post(authorId, caption);
        await using var ctx = CreateContext();
        ctx.Posts.Add(post);
        await ctx.SaveChangesAsync();
        return post.Id;
    }


    [Fact]
    public async Task CreatePost_WithTwoPhotos_PersistsPostAndMediaRows()
    {
        // Arrange
        var author = await SeedUserAsync("author");
        var fileStorage = FileStorageReturning("https://cdn/photo.jpg");
        var dto = new PostCreateDto
        {
            Caption = "Holiday!",
            MediaFiles = new List<IFormFile> { TestHelpers.CreateFormFile("1.jpg"), TestHelpers.CreateFormFile("2.jpg") }
        };

        // Act — use one context for the operation...
        await using (var actCtx = CreateContext())
        {
            var sut = CreateSut(actCtx, fileStorage.Object);
            await sut.CreatePostAsync(dto, author.Id);
        }

        // Assert — ...and a FRESH context to prove it really hit the database.
        await using var assertCtx = CreateContext();
        var post = await assertCtx.Posts.Include(p => p.MediaItems).SingleAsync();
        post.AuthorId.Should().Be(author.Id);
        post.Caption.Should().Be("Holiday!");
        post.MediaItems.Should().HaveCount(2);
        post.MediaItems.Should().OnlyContain(m => m.MediaUrl == "https://cdn/photo.jpg" && m.PostId == post.Id);
        // And the external upload was actually invoked once per file.
        fileStorage.Verify(s => s.UploadFileAsync(It.IsAny<IFormFile>()), Times.Exactly(2));
    }


    [Fact]
    public async Task DeletePost_HidesItFromAllQueries()
    {
        // Arrange
        var author = await SeedUserAsync("author");
        var postId = await SeedPostAsync(author.Id, "to be deleted");

        // Act
        await using (var actCtx = CreateContext())
        {
            var sut = CreateSut(actCtx, FileStorageReturning("x").Object);
            await sut.DeletePostAsync(postId, author.Id);
        }

        // Assert
        await using var assertCtx = CreateContext();
        // A normal query (what a feed uses) no longer sees it — the query filter hides IsDeleted rows.
        (await assertCtx.Posts.AnyAsync(p => p.Id == postId)).Should().BeFalse();
    }


    [Fact]
    public async Task DeletePost_ByNonOwner_Throws_AndLeavesPostUntouched()
    {
        // Arrange
        var author = await SeedUserAsync("author");
        var attacker = await SeedUserAsync("attacker");
        var postId = await SeedPostAsync(author.Id, "not yours");

        // Act
        await using (var actCtx = CreateContext())
        {
            var sut = CreateSut(actCtx, FileStorageReturning("x").Object);
            var act = async () => await sut.DeletePostAsync(postId, attacker.Id);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        // Assert: the post is still live and visible.
        await using var assertCtx = CreateContext();
        (await assertCtx.Posts.AnyAsync(p => p.Id == postId && !p.IsDeleted)).Should().BeTrue();
    }


    [Fact]
    public async Task LikePost_Twice_IsRejected_AndOnlyOneLikeRowExists()
    {
        // Arrange
        var author = await SeedUserAsync("author");
        var liker = await SeedUserAsync("liker");
        var postId = await SeedPostAsync(author.Id, "likeable");

        // Act: first like succeeds; second like is rejected by the service.
        await using (var actCtx = CreateContext())
        {
            var sut = CreateSut(actCtx, FileStorageReturning("x").Object);
            await sut.LikePostAsync(liker.Id, postId);
        }
        await using (var actCtx2 = CreateContext())
        {
            var sut = CreateSut(actCtx2, FileStorageReturning("x").Object);
            var act = async () => await sut.LikePostAsync(liker.Id, postId);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Already liked.");
        }

        // Assert: exactly one like persisted.
        await using var assertCtx = CreateContext();
        (await assertCtx.PostLikes.IgnoreQueryFilters().CountAsync(pl => pl.PostId == postId)).Should().Be(1);
    }


    [Fact]
    public async Task LikePost_OnAnothersPost_PersistsLike_AndNotificationForAuthor()
    {
        // Arrange
        var author = await SeedUserAsync("author");
        var liker = await SeedUserAsync("liker");
        var postId = await SeedPostAsync(author.Id, "notify me");

        // Act
        await using (var actCtx = CreateContext())
        {
            var sut = CreateSut(actCtx, FileStorageReturning("x").Object);
            await sut.LikePostAsync(liker.Id, postId);
        }

        // Assert
        await using var assertCtx = CreateContext();
        (await assertCtx.PostLikes.IgnoreQueryFilters()
            .AnyAsync(pl => pl.PostId == postId && pl.UserId == liker.Id)).Should().BeTrue();

        var notification = await assertCtx.Notifications.IgnoreQueryFilters().SingleAsync();
        notification.RecipientId.Should().Be(author.Id); // the post's author is notified
        notification.ActorId.Should().Be(liker.Id);      // by the person who liked
        notification.PostId.Should().Be(postId);
    }
}
