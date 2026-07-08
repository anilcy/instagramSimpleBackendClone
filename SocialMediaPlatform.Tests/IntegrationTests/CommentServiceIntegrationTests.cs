using Microsoft.EntityFrameworkCore;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Data.Repositories;
using SocialMediaPlatform.Entities.Dtos.CommentDtos;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Tests.IntegrationSupport;

namespace SocialMediaPlatform.Tests.IntegrationTests;


public class CommentServiceIntegrationTests : IntegrationTestBase
{
    private CommentService CreateSut(SocialMediaDbContext ctx) =>
        new CommentService(
            new CommentRepository(ctx),
            new NotificationRepository(ctx),
            new PostRepository(ctx),
            new UnitOfWork(ctx),
            Mapper);

    private async Task<Guid> SeedPostAsync(Guid authorId, string caption)
    {
        await using var ctx = CreateContext();
        var post = new Post(authorId, caption);
        ctx.Posts.Add(post);
        await ctx.SaveChangesAsync();
        return post.Id;
    }


    //Someone comments on a post and another user replies to that comment.
    // The post page shows ONE top-level comment with ONE nested reply, not two flat comments
    [Fact]
    public async Task CommentAndReply_AreReturnedAsAHierarchy_NotAsTwoTopLevelComments()
    {
        // Arrange
        var author = await SeedUserAsync("author");
        var commenter = await SeedUserAsync("commenter");
        var replier = await SeedUserAsync("replier");
        var postId = await SeedPostAsync(author.Id, "discuss!");

        // Act: top-level comment, then a reply to it — through the real service.
        Guid topLevelId;
        await using (var ctx = CreateContext())
        {
            var dto = await CreateSut(ctx).AddCommentAsync(
                new CommentCreateDto { PostId = postId, Content = "first!" }, commenter.Id);
            topLevelId = dto.Id;
        }
        await using (var ctx = CreateContext())
        {
            await CreateSut(ctx).AddCommentAsync(
                new CommentCreateDto { PostId = postId, Content = "replying", ParentCommentId = topLevelId },
                replier.Id);
        }

        // Assert: repository returns 1 top-level comment carrying 1 nested reply.
        await using (var assertCtx = CreateContext())
        {
            var tree = await new CommentRepository(assertCtx).GetCommentsByPostIdAsync(postId, 1, 20);
            tree.Should().HaveCount(1); // the reply is NOT a second top-level row
            tree[0].Content.Should().Be("first!");
            tree[0].Replies.Should().ContainSingle(r => r.Content == "replying");
            tree[0].Replies.Single().Author.Id.Should().Be(replier.Id); // ThenInclude loaded the reply author
        }
    }
    
    // Verifies the reply-notification row really lands(for the comment owner not the post owner), addressed 
    // to the parent comment's author
    [Fact]
    public async Task ReplyToComment_PersistsNotificationForParentCommentAuthor()
    {
        // Arrange: post by author; top-level comment by commenter.
        var author = await SeedUserAsync("author");
        var commenter = await SeedUserAsync("commenter");
        var replier = await SeedUserAsync("replier");
        var postId = await SeedPostAsync(author.Id, "p");
        Guid topLevelId;
        await using (var ctx = CreateContext())
        {
            var dto = await CreateSut(ctx).AddCommentAsync(
                new CommentCreateDto { PostId = postId, Content = "comment" }, commenter.Id);
            topLevelId = dto.Id;
        }

        // Act: replier replies to commenter's comment.
        await using (var ctx = CreateContext())
        {
            await CreateSut(ctx).AddCommentAsync(
                new CommentCreateDto { PostId = postId, Content = "reply", ParentCommentId = topLevelId },
                replier.Id);
        }

        // Assert: exactly the reply notification is addressed to the parent's author.
        await using (var assertCtx = CreateContext())
        {
            var replyNotification = await assertCtx.Notifications.IgnoreQueryFilters()
                .Where(n => n.ActorId == replier.Id)
                .SingleAsync();
            replyNotification.RecipientId.Should().Be(commenter.Id); // parent author, NOT post owner
            replyNotification.PostId.Should().Be(postId);
        }
    }


    [Fact]
    public async Task DeleteComment_HidesItFromThePostPage()
    {
        // Arrange
        var author = await SeedUserAsync("author");
        var commenter = await SeedUserAsync("commenter");
        var postId = await SeedPostAsync(author.Id, "p");
        Guid commentId;
        await using (var ctx = CreateContext())
        {
            var dto = await CreateSut(ctx).AddCommentAsync(
                new CommentCreateDto { PostId = postId, Content = "oops" }, commenter.Id);
            commentId = dto.Id;
        }

        // Act: the commenter deletes their own comment.
        await using (var ctx = CreateContext())
            await CreateSut(ctx).DeleteCommentAsync(commentId, commenter.Id);

        // Assert
        await using (var assertCtx = CreateContext())
        {
            // Gone from the post page query...
            (await new CommentRepository(assertCtx).GetCommentsByPostIdAsync(postId, 1, 20)).Should().BeEmpty();
            // And the post itself is untouched.
            (await assertCtx.Posts.AnyAsync(p => p.Id == postId)).Should().BeTrue();
        }
    }
}
