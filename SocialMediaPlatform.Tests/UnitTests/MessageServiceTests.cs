using AutoMapper;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.MessageDtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Tests.UnitTests;


public class MessageServiceTests
{
    private readonly Mock<IMessageRepository> _messageRepository = new(MockBehavior.Strict);
    private readonly Mock<INotificationRepository> _notificationRepository = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWork = new(MockBehavior.Strict);

    private readonly MessageService _sut;

    public MessageServiceTests()
    {
        _sut = new MessageService(
            _messageRepository.Object,
            _notificationRepository.Object,
            _mapper.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldPersistMessage_Notify_AndReturnDto()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var dto = new MessageCreateDto { ReceiverId = receiverId, Content = "hello" };
        var mapped = new MessageDto();

        _messageRepository.Setup(r => r.Add(It.IsAny<Message>()));
        _notificationRepository.Setup(r => r.Add(It.IsAny<Notification>()));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapper.Setup(m => m.Map<MessageDto>(It.IsAny<Message>())).Returns(mapped);

        // Act
        var result = await _sut.SendMessageAsync(senderId, dto);

        // Assert: message built from the right sender/receiver, receiver notified, one commit.
        result.Should().BeSameAs(mapped);
        _messageRepository.Verify(r => r.Add(It.Is<Message>(m =>
            m.SenderId == senderId && m.ReceiverId == receiverId && m.Content == "hello")), Times.Once);
        _notificationRepository.Verify(r => r.Add(It.IsAny<Notification>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldThrow_WhenMessageNotFound()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        _messageRepository.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync((Message?)null);

        // Act
        var act = async () => await _sut.MarkAsReadAsync(messageId, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Message not found.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldThrow_WhenReaderIsNotTheReceiver()
    {
        // Arrange: the message was sent to someone else, so this reader can't mark it read.
        var messageId = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "hi"); // receiver != readerId
        _messageRepository.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);

        // Act
        var act = async () => await _sut.MarkAsReadAsync(messageId, readerId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only mark your own messages as read.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldMarkRead_AndSave_WhenReaderIsReceiver()
    {
        // Arrange: reader IS the receiver.
        var messageId = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var message = new Message(Guid.NewGuid(), readerId, "hi"); // receiver == readerId
        _messageRepository.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.MarkAsReadAsync(messageId, readerId);

        // Assert
        message.IsRead.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EditMessageAsync_ShouldThrow_WhenNotSender()
    {
        // Arrange: someone other than the sender tries to edit.
        var messageId = Guid.NewGuid();
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "original"); // sender != caller
        _messageRepository.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);

        // Act
        var act = async () => await _sut.EditMessageAsync(messageId, Guid.NewGuid(), "hacked");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only edit your own messages.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EditMessageAsync_ShouldUpdateContent_AndSave_WhenSender()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var message = new Message(senderId, Guid.NewGuid(), "original");
        _messageRepository.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.EditMessageAsync(messageId, senderId, "edited");

        // Assert
        message.Content.Should().Be("edited");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMessageAsync_ShouldThrow_WhenNotSender()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "x"); // sender != caller
        _messageRepository.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);

        // Act
        var act = async () => await _sut.DeleteMessageAsync(messageId, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only delete your own messages.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMessageAsync_ShouldSoftDelete_AndSave_WhenSender()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var message = new Message(senderId, Guid.NewGuid(), "x");
        _messageRepository.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.DeleteMessageAsync(messageId, senderId);

        // Assert
        message.IsDeleted.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkConversationAsReadAsync_ShouldMarkAllUnread_AndSaveOnce()
    {
        // Arrange: two unread messages received from the other user.
        var userId = Guid.NewGuid();
        var fromUserId = Guid.NewGuid();
        var m1 = new Message(fromUserId, userId, "a");
        var m2 = new Message(fromUserId, userId, "b");
        _messageRepository.Setup(r => r.GetUnreadFromUserAsync(userId, fromUserId))
            .ReturnsAsync(new List<Message> { m1, m2 });
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);

        // Act
        await _sut.MarkConversationAsReadAsync(userId, fromUserId);

        // Assert
        m1.IsRead.Should().BeTrue();
        m2.IsRead.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ShouldReturnRepoValue()
    {
        // Arrange (pure delegation smoke test)
        var userId = Guid.NewGuid();
        var fromUserId = Guid.NewGuid();
        _messageRepository.Setup(r => r.GetUnreadMessagesCountAsync(userId, fromUserId)).ReturnsAsync(4);

        // Act
        var count = await _sut.GetUnreadCountAsync(userId, fromUserId);

        // Assert
        count.Should().Be(4);
    }
}
