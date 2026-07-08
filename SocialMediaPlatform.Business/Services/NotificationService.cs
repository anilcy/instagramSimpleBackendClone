using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.NotificationDtos;

namespace SocialMediaPlatform.Business.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(
        INotificationRepository notificationRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, page, pageSize);
        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public async Task<int> GetUnreadNotificationsCountAsync(Guid userId)
    {
        return await _notificationRepository.GetUnreadNotificationsCountAsync(userId);
    }

    public async Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification == null )
            throw new KeyNotFoundException("Notification not found");
        
        notification.MarkAsRead();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAllNotificationsAsReadAsync(Guid userId)
    {
        var notifications = await _notificationRepository.GetUnreadNotificationsByUserAsync(userId);
        foreach (var notification in notifications)
            notification.MarkAsRead();

        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
        var notification = await _notificationRepository.GetNotificationByIdAndRecipientAsync(notificationId, userId);
        if (notification == null)
            throw new KeyNotFoundException("Notification not found.");

        notification.SoftDeleteNotification();
        await _unitOfWork.SaveChangesAsync();
    }
}
    
    