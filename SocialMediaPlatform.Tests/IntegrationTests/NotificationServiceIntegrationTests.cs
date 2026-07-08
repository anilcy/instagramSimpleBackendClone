using Microsoft.EntityFrameworkCore;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Data.Repositories;
using SocialMediaPlatform.Entities.Models;
using SocialMediaPlatform.Tests.IntegrationSupport;

namespace SocialMediaPlatform.Tests.IntegrationTests;


public class NotificationServiceIntegrationTests : IntegrationTestBase
{
    private NotificationService CreateSut(SocialMediaDbContext ctx) =>
        new NotificationService(
            new NotificationRepository(ctx),
            Mapper,
            new UnitOfWork(ctx));

    // Arrange helper: a persisted unread notification for `recipient`, triggered by `actor`.
    private async Task<Notification> SeedNotificationAsync(Guid recipientId, Guid actorId)
    {
        await using var ctx = CreateContext();
        var notification = Notification.MessageNotification(recipientId, actorId);
        ctx.Notifications.Add(notification);
        await ctx.SaveChangesAsync();
        return notification;
    }


    [Fact]
    public async Task UnreadList_ExcludesReadAndDeletedNotifications()
    {
        // Arrange: alice has three notifications: one she read, one she deleted,
        // one untouched.
        var alice = await SeedUserAsync("alice");
        var actor = await SeedUserAsync("actor");
        var readOne = await SeedNotificationAsync(alice.Id, actor.Id);
        var deletedOne = await SeedNotificationAsync(alice.Id, actor.Id);
        var freshOne = await SeedNotificationAsync(alice.Id, actor.Id);

        await using (var ctx = CreateContext())
            await CreateSut(ctx).MarkNotificationAsReadAsync(readOne.Id, alice.Id);
        await using (var ctx = CreateContext())
            await CreateSut(ctx).DeleteNotificationAsync(deletedOne.Id, alice.Id);

        // Act + Assert: only the untouched one is still "unread".
        await using var assertCtx = CreateContext();
        var unread = await new NotificationRepository(assertCtx).GetUnreadNotificationsByUserAsync(alice.Id);
        unread.Should().ContainSingle(n => n.Id == freshOne.Id);
    }

  
    [Fact]
    public async Task MarkAllAsRead_OnlyAffectsThatUsersNotifications()
    {
        // Arrange: alice has two unread, bob has one unread.
        var alice = await SeedUserAsync("alice");
        var bob = await SeedUserAsync("bob");
        var actor = await SeedUserAsync("actor");
        await SeedNotificationAsync(alice.Id, actor.Id);
        await SeedNotificationAsync(alice.Id, actor.Id);
        await SeedNotificationAsync(bob.Id, actor.Id);

        // Act: alice clicks "mark all as read".
        await using (var ctx = CreateContext())
            await CreateSut(ctx).MarkAllNotificationsAsReadAsync(alice.Id);

        // Assert: alice's badge is cleared, bob's badge is untouched — the isolation
        // claim needs BOTH sides: her count dropped to 0 AND his stayed exactly 1.
        await using var assertCtx = CreateContext();
        var sut = CreateSut(assertCtx);
        (await sut.GetUnreadNotificationsCountAsync(alice.Id)).Should().Be(0);
        (await sut.GetUnreadNotificationsCountAsync(bob.Id)).Should().Be(1);
    }


    [Fact]
    public async Task DeleteNotification_WithSomeoneElsesId_IsRejected_AndNothingIsDeleted()
    {
        // Arrange: a notification that belongs to alice.
        var alice = await SeedUserAsync("alice");
        var bob = await SeedUserAsync("bob");
        var actor = await SeedUserAsync("actor");
        var aliceNotification = await SeedNotificationAsync(alice.Id, actor.Id);

        // Act: bob attempts to delete alice's notification using its real ID.
        await using (var ctx = CreateContext())
        {
            var act = async () => await CreateSut(ctx).DeleteNotificationAsync(aliceNotification.Id, bob.Id);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Notification not found.");
        }

        // And alice's notification is fully intact (not even soft-deleted).
        await using var assertCtx = CreateContext();
        (await assertCtx.Notifications.AnyAsync(n => n.Id == aliceNotification.Id)).Should().BeTrue();
    }
    
    [Fact]
    public async Task ReadingOneNotification_DropsUnreadCountByExactlyOne()
    {
        // Arrange: two unread notifications.
        var alice = await SeedUserAsync("alice");
        var actor = await SeedUserAsync("actor");
        var first = await SeedNotificationAsync(alice.Id, actor.Id);
        await SeedNotificationAsync(alice.Id, actor.Id);

        await using (var ctx = CreateContext())
            (await CreateSut(ctx).GetUnreadNotificationsCountAsync(alice.Id)).Should().Be(2); // sanity

        // Act: alice opens the first one.
        await using (var ctx = CreateContext())
            await CreateSut(ctx).MarkNotificationAsReadAsync(first.Id, alice.Id);

        // Assert: the count reflects the persisted read flag.
        await using var assertCtx = CreateContext();
        (await CreateSut(assertCtx).GetUnreadNotificationsCountAsync(alice.Id)).Should().Be(1);
    }
}
