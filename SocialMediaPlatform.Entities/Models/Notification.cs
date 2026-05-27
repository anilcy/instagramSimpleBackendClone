using System;
using SocialMediaPlatform.Entities.Dtos;

namespace SocialMediaPlatform.Entities.Models;

public enum NotificationType
{
    PostLike,           // Someone liked your post
    Message,
    Follow,         // Someone followed you
    FollowRequest,  // Someone requested to follow you
    Comment,        // Someone commented on your post
    CommentLike,    // Someone liked your comment
    CommentReply    // Someone replied to your comment
}
public class Notification
{
    public Notification(Guid recipientId, NotificationType type, string message)
    {
        if (recipientId == Guid.Empty)
            throw new ArgumentException("Invalid recipient id.", nameof(recipientId));
        
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty.", nameof(message));
        
        RecipientId = recipientId;
        Type = type;
        Message = message;
        CreatedAt = DateTimeOffset.UtcNow;
        IsRead = false;
        IsDeleted = false;
    }
    public static Notification PostLikeNotification(Guid recipientId, Guid actorId, Guid postId)
    {
        // client side will put the actor's name 
        var message = "liked your post.";
        var notification = new Notification(recipientId, NotificationType.PostLike, message);
        notification.ActorId = actorId;
        notification.PostId = postId;
        return notification;
    }
    
    public static Notification MessageNotification(Guid recipientId, Guid actorId)
    {
        var message = "sent you a message.";
        var notification = new Notification(recipientId, NotificationType.Message, message);
        notification.ActorId = actorId;
        return notification;
    }

    public static Notification FollowNotification(Guid recipientId, Guid actorId)
    {
        var message = "started following you.";
        var notification = new Notification(recipientId, NotificationType.Follow, message);
        notification.ActorId = actorId;
        return notification;
    }

    public static Notification FollowRequestNotification(Guid recipientId, Guid actorId)
    {
        var message = "wants to follow you.";
        var notification = new Notification(recipientId, NotificationType.FollowRequest, message);
        notification.ActorId = actorId;
        return notification;
    }
    
    public static Notification CommentNotification(Guid recipientId, Guid actorId, Guid commentId, Guid postId)
    {
        var message = "commented on your post.";
        var notification = new Notification(recipientId, NotificationType.Comment, message);
        notification.ActorId = actorId;
        notification.PostId = postId;
        notification.CommentId = commentId;
        return notification;
    }

    public static Notification CommentLikeNotification(Guid recipientId, Guid commentId, Guid actorId)
    {
        var message = "liked your comment.";
        var notification = new Notification(recipientId, NotificationType.CommentLike, message);
        notification.ActorId = actorId;
        notification.CommentId = commentId;
        return notification;
    }

    public static Notification CommentReplyNotification(Guid recipientId, Guid actorId, Guid postId, Guid commentId)
    {
        var message = "replied to your comment.";
        var notification = new Notification(recipientId, NotificationType.CommentReply, message);
        notification.ActorId = actorId;
        notification.PostId = postId;
        notification.CommentId = commentId;
        return notification;
    }
    
    private Notification() { }
    public Guid Id { get; private set; }
    public Guid RecipientId { get; private set; } 
    public NotificationType Type { get; private set; }
    public string Message { get; private set; }
    public string? ActionUrl { get; private set; }
    public bool IsRead { get; private set; } 
    public DateTimeOffset CreatedAt { get; private set; } 
    public bool IsDeleted { get; private set; } 
    
    // Related entities (optional, depending on notification type)
    public Guid? ActorId { get; private set; }  // User who triggered the notification
    public Guid? PostId { get; private set; }
    public Guid? CommentId { get; private set; }
    
    public AppUser Recipient { get; private set; }
    public AppUser? Actor { get; private set; }
    public Post? Post { get; private set; }
    public Comment? Comment { get; private set; }
    
    public void MarkAsRead()
    {
        if (IsRead)
            return;
        IsRead = true;
    }
    
    public void SoftDeleteNotification()
    {
        if (IsDeleted)
            return;
        IsDeleted = true;
    }
}