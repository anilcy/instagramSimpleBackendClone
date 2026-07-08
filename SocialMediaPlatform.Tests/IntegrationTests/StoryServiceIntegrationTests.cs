using Microsoft.EntityFrameworkCore;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Data.Repositories;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Tests.IntegrationSupport;

namespace SocialMediaPlatform.Tests.IntegrationTests;


public class StoryServiceIntegrationTests : IntegrationTestBase
{
    private StoryService CreateSut(SocialMediaDbContext ctx) =>
        new StoryService(
            new StoryRepository(ctx),
            new NotificationRepository(ctx),
            new Mock<IFileStorageService>().Object, // external world — not under test
            Mapper,
            new PrivacyService(new UserRepository(ctx), new FollowRepository(ctx)),
            new UnitOfWork(ctx));

    // Insert a story row directly (arrange helper).
    private async Task<Story> SeedStoryAsync(Guid userId, string url = "https://cdn/s.jpg")
    {
        await using var ctx = CreateContext();
        var story = new Story(userId, url);
        ctx.Stories.Add(story);
        await ctx.SaveChangesAsync();
        return story;
    }


    [Fact]
    public async Task ExpiredStory_DoesNotAppearInActiveStories()
    {
        // Arrange: a story created 2 days ago, so it is long past its 24h window.
        var user = await SeedUserAsync("author");
        var story = await SeedStoryAsync(user.Id);
        await using (var ctx = CreateContext())
        {
            var tracked = await ctx.Stories.IgnoreQueryFilters().SingleAsync(s => s.Id == story.Id);
            ctx.Entry(tracked).Property(nameof(Story.ExpiresAt)).CurrentValue =
                DateTimeOffset.UtcNow.AddDays(-2);
            await ctx.SaveChangesAsync();
        }

        // Act: open the author's profile stories.
        await using var assertCtx = CreateContext();
        var stories = await CreateSut(assertCtx).GetUserActiveStoriesAsync(user.Id, user.Id);

        // Assert: the expired story is not shown.
        stories.Should().BeEmpty();
    }


    [Fact]
    public async Task DeactivatedUsersStory_IsHiddenFromEveryone()
    {
        // Arrange: an active user with a live story.
        var user = await SeedUserAsync("sleeper");
        await SeedStoryAsync(user.Id);

        // Act: the user deactivates their account.
        await using (var ctx = CreateContext())
        {
            (await ctx.Users.SingleAsync(u => u.Id == user.Id)).DeactivateAccount();
            await ctx.SaveChangesAsync();
        }

        // Assert
        await using var assertCtx = CreateContext();
        (await assertCtx.Stories.AnyAsync()).Should().BeFalse();// if the story is hidden from everyone(AnyAsync checks if there are stories visible)
    }
   
    [Fact]
    public async Task StoryFeed_ShowsOwnAndFollowedUsersStories()
    {
        // Arrange
        var me = await SeedUserAsync("me");
        var friend = await SeedUserAsync("friend");
        var idol = await SeedUserAsync("idol");
        var stranger = await SeedUserAsync("stranger");
        await using (var ctx = CreateContext())
        {
            (await ctx.Users.SingleAsync(u => u.Id == idol.Id)).SetPrivate(true);
            ctx.Follows.Add(new Follow(me.Id, friend.Id, isPrivate: false)); // accepted
            ctx.Follows.Add(new Follow(me.Id, idol.Id, isPrivate: true));    // my pending request
            await ctx.SaveChangesAsync();
        }
        await SeedStoryAsync(me.Id);
        await SeedStoryAsync(friend.Id);
        await SeedStoryAsync(idol.Id);
        await SeedStoryAsync(stranger.Id);

        // Act: load my story feed.
        await using var assertCtx = CreateContext();
        var feed = await CreateSut(assertCtx).GetStoriesFeedAsync(me.Id);

        // Assert: mine + friend's + idol's (I follow them after all) — the stranger's
        // story must never appear in my tray.
        feed.Should().HaveCount(2);
        feed.Select(s => s.UserId).Should().NotContain(stranger.Id);
    }


    [Fact]
    public async Task PrivateUsersStories_BlockedForStranger_VisibleToAcceptedFollower()
    {
        // Arrange: a private account with one story, and a stranger.
        var target = await SeedUserAsync("target");
        var stranger = await SeedUserAsync("stranger");
        await using (var ctx = CreateContext())
        {
            (await ctx.Users.SingleAsync(u => u.Id == target.Id)).SetPrivate(true);
            await ctx.SaveChangesAsync();
        }
        await SeedStoryAsync(target.Id);

        // Act + Assert 1: the stranger is rejected by the real privacy chain.
        await using (var ctx = CreateContext())
        {
            var act = async () => await CreateSut(ctx).GetUserActiveStoriesAsync(target.Id, stranger.Id);
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User's content is private");
        }

        // Arrange 2: the relationship becomes an accepted follow.
        await using (var ctx = CreateContext())
        {
            ctx.Follows.Add(new Follow(stranger.Id, target.Id, isPrivate: false)); // accepted
            await ctx.SaveChangesAsync();
        }

        // Act + Assert 2: the same call now succeeds and returns the story.
        await using (var ctx2 = CreateContext())
        {
            var stories = await CreateSut(ctx2).GetUserActiveStoriesAsync(target.Id, stranger.Id);
            stories.Should().ContainSingle();
        }
    }
}
