using AutoMapper;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.NotificationDtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Tests.UnitTests;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepository = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWork = new(MockBehavior.Strict);

    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(
            _notificationRepository.Object,
            _mapper.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ShouldReturnMappedList()
    {
        // Arrange: pure delegation , repo returns entities, service maps them to DTOs.
        var userId = Guid.NewGuid();
        var entities = new List<Notification> { Notification.MessageNotification(userId, Guid.NewGuid()) };
        var dtos = new List<NotificationDto> { new NotificationDto() };
        _notificationRepository.Setup(r => r.GetUserNotificationsAsync(userId, 1, 20)).ReturnsAsync(entities);
        _mapper.Setup(m => m.Map<List<NotificationDto>>(entities)).Returns(dtos);

        // Act
        var result = await _sut.GetUserNotificationsAsync(userId);

        // Assert: it returns exactly what the mapper produced.
        result.Should().BeSameAs(dtos);
    }

    [Fact]
    public async Task GetUnreadNotificationsCountAsync_ShouldReturnRepoValue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _notificationRepository.Setup(r => r.GetUnreadNotificationsCountAsync(userId)).ReturnsAsync(7);

        // Act
        var count = await _sut.GetUnreadNotificationsCountAsync(userId);

        // Assert
        count.Should().Be(7);
    }

    [Fact]
    public async Task MarkNotificationAsReadAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _notificationRepository.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync((Notification?)null);

        // Act
        var act = async () => await _sut.MarkNotificationAsReadAsync(notificationId, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Notification not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkNotificationAsReadAsync_ShouldMarkRead_AndSave()
    {
        // Arrange: a fresh (unread) notification.
        var notificationId = Guid.NewGuid();
        var notification = Notification.MessageNotification(Guid.NewGuid(), Guid.NewGuid());
        notification.IsRead.Should().BeFalse(); // sanity: starts unread
        _notificationRepository.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.MarkNotificationAsReadAsync(notificationId, Guid.NewGuid());

        // Assert: the domain object transitioned to read, and we committed once.
        notification.IsRead.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAllNotificationsAsReadAsync_ShouldMarkEveryUnread_AndSaveOnce()
    {
        // Arrange: two unread notifications for the user.
        var userId = Guid.NewGuid();
        var n1 = Notification.MessageNotification(userId, Guid.NewGuid());
        var n2 = Notification.FollowNotification(userId, Guid.NewGuid());
        _notificationRepository.Setup(r => r.GetUnreadNotificationsByUserAsync(userId))
            .ReturnsAsync(new List<Notification> { n1, n2 });
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);

        // Act
        await _sut.MarkAllNotificationsAsReadAsync(userId);

        // Assert: all flipped to read, single commit for the whole batch.
        n1.IsRead.Should().BeTrue();
        n2.IsRead.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteNotificationAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _notificationRepository.Setup(r => r.GetNotificationByIdAndRecipientAsync(notificationId, userId))
            .ReturnsAsync((Notification?)null);

        // Act
        var act = async () => await _sut.DeleteNotificationAsync(notificationId, userId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Notification not found.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteNotificationAsync_ShouldSoftDelete_AndSave()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var notification = Notification.MessageNotification(userId, Guid.NewGuid());
        _notificationRepository.Setup(r => r.GetNotificationByIdAndRecipientAsync(notificationId, userId))
            .ReturnsAsync(notification);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.DeleteNotificationAsync(notificationId, userId);

        // Assert
        notification.IsDeleted.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
