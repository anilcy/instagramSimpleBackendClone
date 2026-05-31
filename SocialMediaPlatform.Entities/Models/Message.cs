using System;

namespace SocialMediaPlatform.Entities.Models;

public class Message
{
    //Content can be null in case a media is sent without a message and there should be a check if at least one of
    //them is provided, the check will be in the service since they will be set at different times separately.
    public Message (Guid senderId, Guid receiverId, string? content)
    {
        if (senderId == Guid.Empty)
            throw new ArgumentException("Sender Id cannot be empty.", nameof(senderId));
        
        if (receiverId == Guid.Empty)
            throw new ArgumentException("Receiver Id cannot be empty.", nameof(receiverId));
        
        if (senderId == receiverId)
            throw new ArgumentException("Sender and receiver cannot be the same.");
        
        SenderId = senderId;
        ReceiverId = receiverId;
        Content = content;
        CreatedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
        IsRead = false;
    }
    private Message() { }
    
    public Guid Id { get; private set; }
    public Guid SenderId { get; private set; }
    public Guid ReceiverId { get; private set; }
    public string? Content { get; private set; } 
    public DateTimeOffset CreatedAt { get; private set; } 
    public DateTimeOffset? ReadAt { get; private set; } 
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    //Status flags
    public bool IsRead { get; private set; } 
    public bool IsDeleted { get; private set; } 
    

  
    //Navigation properties 
    public AppUser Receiver { get; private set; } = null!;
    public AppUser Sender { get; private set; } = null!;
    public ICollection<Media> MediaItems { get; private set; } = new List<Media>();
    
    public void MarkAsRead()
    {
        if (IsRead)
            return;
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow; 
    }
    public void EditMessage (string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Content cannot be empty.", nameof(newContent));
        Content = newContent;
        
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void SoftDeleteMessage()
    {
        if (IsDeleted)
            return;
        IsDeleted = true;
    }
    
}