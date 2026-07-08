using Microsoft.EntityFrameworkCore;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Data.Repositories;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Tests.IntegrationSupport;

namespace SocialMediaPlatform.Tests.IntegrationTests;

public class FollowServiceIntegrationTests : IntegrationTestBase
{
    // Real FollowService wired onto a given context, no mocks at all here.
    private FollowService CreateSut(SocialMediaDbContext ctx) =>
        new FollowService(
            new FollowRepository(ctx),
            new NotificationRepository(ctx),
            new UserRepository(ctx),
            Mapper,
            new UnitOfWork(ctx));
    
    
    [Fact]
    public async Task PrivateAccount_FollowRequestLifecycle_PendingThenAccepted()
    {
        // Arrange: a private target and a requester.
        var requester = await SeedUserAsync("requester");
        var target = await SeedUserAsync("target");
        await using (var ctx = CreateContext())
        {
            var u = await ctx.Users.SingleAsync(x => x.Id == target.Id);
            u.SetPrivate(true);
            await ctx.SaveChangesAsync();
        }

        // Act 1: request to follow.
        await using (var ctx = CreateContext())
            await CreateSut(ctx).FollowUserAsync(requester.Id, target.Id);

        // Assert 1: row persisted as Pending; NOT yet a follower anywhere that matters.
        await using (var ctx = CreateContext())
        {
            var repo = new FollowRepository(ctx);
            (await ctx.Follows.SingleAsync()).Status.Should().Be(FollowStatus.Pending);
            (await repo.IsFollowingAsync(requester.Id, target.Id)).Should().BeFalse();   // Accepted-only query
            (await repo.GetFollowersCountAsync(target.Id)).Should().Be(0);               // count ignores Pending
            (await repo.GetPendingFollowRequestsAsync(target.Id, 1, 20)).Should().HaveCount(1); // but it IS in the inbox
        }

        // Act 2: the owner accepts the request.
        await using (var ctx = CreateContext())
            await CreateSut(ctx).RespondToFollowRequestAsync(target.Id, requester.Id, FollowStatus.Accepted);

        // Assert 2: NOW the requester is a real follower, visible in lists and counts.
        await using (var ctx = CreateContext())
        {
            var repo = new FollowRepository(ctx);
            (await repo.IsFollowingAsync(requester.Id, target.Id)).Should().BeTrue();
            (await repo.GetFollowersCountAsync(target.Id)).Should().Be(1);
            var followers = await repo.GetFollowersAsync(target.Id, 1, 20);
            followers.Should().ContainSingle(f => f.FollowerId == requester.Id);
            (await repo.GetPendingFollowRequestsAsync(target.Id, 1, 20)).Should().BeEmpty(); // inbox cleared
        }
    }


    [Fact]
    public async Task PrivateAccount_RejectedRequest_NeverBecomesFollower()
    {
        // Arrange
        var requester = await SeedUserAsync("requester");
        var target = await SeedUserAsync("target");
        await using (var ctx = CreateContext())
        {
            (await ctx.Users.SingleAsync(x => x.Id == target.Id)).SetPrivate(true);
            await ctx.SaveChangesAsync();
        }
        await using (var ctx = CreateContext())
            await CreateSut(ctx).FollowUserAsync(requester.Id, target.Id);

        // Act: reject.
        await using (var ctx = CreateContext())
            await CreateSut(ctx).RespondToFollowRequestAsync(target.Id, requester.Id, FollowStatus.Rejected);

        // Assert
        await using (var ctx = CreateContext())
        {
            var repo = new FollowRepository(ctx);
            (await ctx.Follows.SingleAsync()).Status.Should().Be(FollowStatus.Rejected);
            (await repo.IsFollowingAsync(requester.Id, target.Id)).Should().BeFalse();
            (await repo.GetFollowersCountAsync(target.Id)).Should().Be(0);
        }
    }
    
    
    [Fact]
    public async Task Unfollow_MakesAllQueriesForgetTheRelationship()
    {
        // Arrange: a public account, follow is immediately Accepted.
        var follower = await SeedUserAsync("follower");
        var followed = await SeedUserAsync("followed");
        await using (var ctx = CreateContext())
            await CreateSut(ctx).FollowUserAsync(follower.Id, followed.Id);

        // Act: unfollow.
        await using (var ctx = CreateContext())
            (await CreateSut(ctx).UnfollowUserAsync(follower.Id, followed.Id)).Should().BeTrue();

        // Assert: every query now behaves as if the relationship never existed...
        await using (var ctx = CreateContext())
        {
            var repo = new FollowRepository(ctx);
            (await repo.IsFollowingAsync(follower.Id, followed.Id)).Should().BeFalse();
            (await repo.GetFollowersCountAsync(followed.Id)).Should().Be(0);
            (await repo.GetFollowRelationshipAsync(follower.Id, followed.Id)).Should().BeNull(); // filter hides it
        }
    }
    
    [Fact]
    public async Task DeactivatedFollower_DisappearsFromFollowerListAndCount()
    {
        // Arrange: an accepted follow.
        var follower = await SeedUserAsync("follower");
        var followed = await SeedUserAsync("followed");
        await using (var ctx = CreateContext())
            await CreateSut(ctx).FollowUserAsync(follower.Id, followed.Id);
        await using (var ctx = CreateContext())
            (await new FollowRepository(ctx).GetFollowersCountAsync(followed.Id)).Should().Be(1); // sanity

        // Act: the follower deactivates their account.
        await using (var ctx = CreateContext())
        {
            (await ctx.Users.SingleAsync(u => u.Id == follower.Id)).DeactivateAccount();
            await ctx.SaveChangesAsync();
        }

        // Assert: without touching the Follow row at all, they vanished from queries.
        await using (var ctx = CreateContext())
        {
            var repo = new FollowRepository(ctx);
            (await repo.GetFollowersCountAsync(followed.Id)).Should().Be(0);
            (await repo.GetFollowersAsync(followed.Id, 1, 20)).Should().BeEmpty();
            (await repo.IsFollowingAsync(follower.Id, followed.Id)).Should().BeFalse();
        }
    }


    [Fact]
    public async Task PrivateAccountPosts_HiddenFromStrangers_VisibleToAcceptedFollowers()
    {
        // Arrange: private author with one post.
        var author = await SeedUserAsync("author");
        var stranger = await SeedUserAsync("stranger");
        await using (var ctx = CreateContext())
        {
            (await ctx.Users.SingleAsync(x => x.Id == author.Id)).SetPrivate(true);
            ctx.Posts.Add(new Post(author.Id, "secret life"));
            await ctx.SaveChangesAsync();
        }

        PostService CreatePostSut(SocialMediaDbContext ctx) => new PostService(
            new PostRepository(ctx),
            new Mock<Business.Interfaces.IFileStorageService>().Object, // external world — irrelevant here
            new MediaRepository(ctx),
            new NotificationRepository(ctx),
            Mapper,
            new PrivacyService(new UserRepository(ctx), new FollowRepository(ctx)), // REAL privacy chain
            new UnitOfWork(ctx));

        // Act + Assert 1: the stranger is rejected.
        await using (var ctx = CreateContext())
        {
            var act = async () => await CreatePostSut(ctx).GetPostsAsync(author.Id, stranger.Id, 1, 20);
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User's content is private");
        }

        // Arrange 2: stranger requests + author accepts (via the real service).
        await using (var ctx = CreateContext())
            await CreateSut(ctx).FollowUserAsync(stranger.Id, author.Id);
        await using (var ctx = CreateContext())
            await CreateSut(ctx).RespondToFollowRequestAsync(author.Id, stranger.Id, FollowStatus.Accepted);

        // Act + Assert 2: the same call now succeeds and returns the post.
        await using (var ctx = CreateContext())
        {
            var posts = await CreatePostSut(ctx).GetPostsAsync(author.Id, stranger.Id, 1, 20);
            posts.Should().ContainSingle();
        }
    }
    
    [Fact]
    public async Task Feed_ContainsOwnAndAcceptedFollowsPosts_NotPendingOrStrangers()
    {
        // Arrange: me, a public friend (accepted), a private idol (pending), a stranger.
        var me = await SeedUserAsync("me");
        var friend = await SeedUserAsync("friend");
        var idol = await SeedUserAsync("idol");
        var stranger = await SeedUserAsync("stranger");
        await using (var ctx = CreateContext())
        {
            (await ctx.Users.SingleAsync(x => x.Id == idol.Id)).SetPrivate(true);
            ctx.Posts.AddRange(
                new Post(me.Id, "my post"),
                new Post(friend.Id, "friend post"),
                new Post(idol.Id, "idol post"),
                new Post(stranger.Id, "stranger post"));
            await ctx.SaveChangesAsync();
        }
        await using (var ctx = CreateContext())
            await CreateSut(ctx).FollowUserAsync(me.Id, friend.Id);  // → Accepted (public)
        await using (var ctx = CreateContext())
            await CreateSut(ctx).FollowUserAsync(me.Id, idol.Id);    // → Pending (private)

        // Act
        await using var assertCtx = CreateContext();
        var feed = await new PostRepository(assertCtx).GetFeedAsync(me.Id, 1, 20);

        // Assert: own + accepted-friend posts only. Pending and strangers excluded.
        feed.Select(p => p.Caption).Should().BeEquivalentTo(new[] { "my post", "friend post" });
    }
}
