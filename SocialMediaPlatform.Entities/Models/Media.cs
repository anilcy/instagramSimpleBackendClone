namespace SocialMediaPlatform.Entities.Models;

public enum MediaType
{
    Image,
    Video,
    Audio,
    File
}

public class Media
{
    public Media(Guid userId, string mediaUrl, MediaType type)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User Id cannot be empty.", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(mediaUrl))
            throw new ArgumentException("Media url cannot be empty.", nameof(mediaUrl));
        
        UserId = userId;
        MediaUrl = mediaUrl;
        Type = type;
        CreatedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }
    private Media() { }
    
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? PostId { get; private set; }
    public Guid? MessageId { get; private set; }
    public string MediaUrl { get; private set; }
    public bool IsDeleted { get; private set; }  
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; } = null;
    public MediaType Type { get; private set; }
    
    //Navigation properties
    public Message? Message { get; private set; }
    public Post? Post { get; private set; }
    public AppUser User { get; private set; } = null!;
    
    public static Media ForPost(Guid userId, Guid postId, string mediaUrl, MediaType type)
    {
        var media = new Media(userId, mediaUrl, type);
        media.PostId = postId;
        return media;
    }

    public static Media ForMessage(Guid userId, Guid messageId, string mediaUrl, MediaType type)
    {
        var media = new Media(userId, mediaUrl, type);
        media.MessageId = messageId;
        return media;
    }
    
    public void SoftDeleteMedia()
    {
        if (IsDeleted)
            return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
    }
}