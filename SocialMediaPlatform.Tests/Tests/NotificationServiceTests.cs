using AutoMapper;
using FluentAssertions;
using SocialMediaPlatform.Business.Services;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Models;
using Moq;

namespace SocialMediaPlatform.Tests.Tests;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepository = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(_notificationRepository.Object, _mapper.Object);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ShouldMapRepositoryResults()
    {
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            new() { Id = 1, RecipientId = userId, Type = NotificationType.Follow, Message = "started following you" },
            new() { Id = 2, RecipientId = userId, Type = NotificationType.Like, Message = "liked your post" }
        };
        var expected = new List<NotificationDto>
        {
            new() { Id = 1, RecipientId = userId, Type = NotificationType.Follow, Message = "started following you" },
            new() { Id = 2, RecipientId = userId, Type = NotificationType.Like, Message = "liked your post" }
        };

        _notificationRepository.Setup(r => r.GetUserNotificationsAsync(userId, 1, 20)).ReturnsAsync(notifications);
        _mapper.Setup(m => m.Map<List<NotificationDto>>(notifications)).Returns(expected);

        var result = await _sut.GetUserNotificationsAsync(userId);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetUnreadNotificationsCountAsync_ShouldReturnCountFromRepository()
    {
        var userId = Guid.NewGuid();

        _notificationRepository.Setup(r => r.GetUnreadNotificationsCountAsync(userId)).ReturnsAsync(7);

        var result = await _sut.GetUnreadNotificationsCountAsync(userId);

        result.Should().Be(7);
    }

    [Fact]
    public async Task CreateLikeNotificationAsync_ShouldIgnoreSelfNotifications()
    {
        var userId = Guid.NewGuid();

        await _sut.CreateLikeNotificationAsync(userId, userId, 5);

        _notificationRepository.Verify(r => r.CreateNotificationAsync(It.IsAny<Guid>(), It.IsAny<NotificationType>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task CreateFollowNotificationAsync_ShouldCreateNotificationForRecipient()
    {
        var actorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        _notificationRepository.Setup(r => r.CreateNotificationAsync(
            recipientId,
            NotificationType.Follow,
            "started following you",
            $"/users/{actorId}",
            actorId,
            null,
            null)).Returns(Task.CompletedTask);

        await _sut.CreateFollowNotificationAsync(actorId, recipientId);

        _notificationRepository.Verify(r => r.CreateNotificationAsync(
            recipientId,
            NotificationType.Follow,
            "started following you",
            $"/users/{actorId}",
            actorId,
            null,
            null), Times.Once);
    }
}

