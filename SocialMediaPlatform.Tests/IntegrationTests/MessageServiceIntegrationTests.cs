using Microsoft.EntityFrameworkCore;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Data.Repositories;
using SocialMediaPlatform.Entities.Dtos.MessageDtos;
using SocialMediaPlatform.Tests.IntegrationSupport;

namespace SocialMediaPlatform.Tests.IntegrationTests;


public class MessageServiceIntegrationTests : IntegrationTestBase
{
    private MessageService CreateSut(SocialMediaDbContext ctx) =>
        new MessageService(
            new MessageRepository(ctx),
            new NotificationRepository(ctx),
            Mapper,
            new UnitOfWork(ctx));
    
    [Fact]
    public async Task Inbox_ShowsOneRowPerPartner_WithLastMessageAndUnreadCount()
    {
        // Arrange: three users; Bob sends Alice two messages, Alice answers Carol once,
        // then Carol sends one back.
        var alice = await SeedUserAsync("alice");
        var bob = await SeedUserAsync("bob");
        var carol = await SeedUserAsync("carol");

        await using (var ctx = CreateContext())
        {
            var sut = CreateSut(ctx);
            await sut.SendMessageAsync(bob.Id, new MessageCreateDto { ReceiverId = alice.Id, Content = "hey" });
            await sut.SendMessageAsync(bob.Id, new MessageCreateDto { ReceiverId = alice.Id, Content = "you there?" });
            await sut.SendMessageAsync(alice.Id, new MessageCreateDto { ReceiverId = carol.Id, Content = "hi carol" });
            await sut.SendMessageAsync(carol.Id, new MessageCreateDto { ReceiverId = alice.Id, Content = "hi alice!" });
        }

        // Act: Alice opens her inbox.
        await using var assertCtx = CreateContext();
        var inbox = await CreateSut(assertCtx).GetConversationsAsync(alice.Id, 1, 20);

        // Assert: one row per partner (2 partners, 4 messages), not one row per message.
        inbox.Should().HaveCount(2);

        var bobRow = inbox.Single(c => c.OtherUser.Id == bob.Id);
        bobRow.LastMessage.Content.Should().Be("you there?"); // the LATEST message wins
        bobRow.UnreadCount.Should().Be(2);                    // Alice read nothing from Bob yet

        var carolRow = inbox.Single(c => c.OtherUser.Id == carol.Id);
        carolRow.LastMessage.Content.Should().Be("hi alice!");
        carolRow.UnreadCount.Should().Be(1); // Alice's OWN outgoing message doesn't count as unread
    }


    [Fact]
    public async Task OpeningAChat_MarksThatConversationRead_OtherChatsUnaffected()
    {
        // Arrange: unread messages from two different partners.
        var alice = await SeedUserAsync("alice");
        var bob = await SeedUserAsync("bob");
        var carol = await SeedUserAsync("carol");
        await using (var ctx = CreateContext())
        {
            var sut = CreateSut(ctx);
            await sut.SendMessageAsync(bob.Id, new MessageCreateDto { ReceiverId = alice.Id, Content = "b1" });
            await sut.SendMessageAsync(bob.Id, new MessageCreateDto { ReceiverId = alice.Id, Content = "b2" });
            await sut.SendMessageAsync(carol.Id, new MessageCreateDto { ReceiverId = alice.Id, Content = "c1" });
        }

        // Act: Alice opens ONLY the Bob chat.
        await using (var ctx = CreateContext())
            await CreateSut(ctx).MarkConversationAsReadAsync(alice.Id, bob.Id);

        // Assert: Bob's messages persisted as read; Carol's untouched.
        await using var assertCtx = CreateContext();
        var sutAssert = CreateSut(assertCtx);
        (await sutAssert.GetUnreadCountAsync(alice.Id, bob.Id)).Should().Be(0);
        (await sutAssert.GetUnreadCountAsync(alice.Id, carol.Id)).Should().Be(1);
        // Double-check at the row level: read flags were really written to the DB.
        (await assertCtx.Messages.CountAsync(m => m.SenderId == bob.Id && m.IsRead)).Should().Be(2);
    }


    [Fact]
    public async Task ChatHistory_MergesBothDirections_InChronologicalOrder()
    {
        // Arrange: an exchange in both directions.
        var alice = await SeedUserAsync("alice");
        var bob = await SeedUserAsync("bob");
        await using (var ctx = CreateContext())
        {
            var sut = CreateSut(ctx);
            await sut.SendMessageAsync(alice.Id, new MessageCreateDto { ReceiverId = bob.Id, Content = "1 hi" });
            await sut.SendMessageAsync(bob.Id, new MessageCreateDto { ReceiverId = alice.Id, Content = "2 hello" });
            await sut.SendMessageAsync(alice.Id, new MessageCreateDto { ReceiverId = bob.Id, Content = "3 how are you" });
        }

        // Act
        await using var assertCtx = CreateContext();
        var history = await CreateSut(assertCtx).GetConversationAsync(alice.Id, bob.Id, 1, 50);

        // Assert: all messages from BOTH directions, oldest first (chat order).
        history.Select(m => m.Content).Should().ContainInOrder("1 hi", "2 hello", "3 how are you");
    }


    [Fact]
    public async Task DeletedMessage_DisappearsFromChatHistory()
    {
        // Arrange
        var alice = await SeedUserAsync("alice");
        var bob = await SeedUserAsync("bob");
        Guid messageId;
        await using (var ctx = CreateContext())
        {
            var dto = await CreateSut(ctx).SendMessageAsync(
                alice.Id, new MessageCreateDto { ReceiverId = bob.Id, Content = "regret this" });
            messageId = dto.Id;
        }

        // Act: sender deletes it.
        await using (var ctx = CreateContext())
            await CreateSut(ctx).DeleteMessageAsync(messageId, alice.Id);

        // Assert
        await using var assertCtx = CreateContext();
        (await CreateSut(assertCtx).GetConversationAsync(alice.Id, bob.Id, 1, 50)).Should().BeEmpty();
    }
}
