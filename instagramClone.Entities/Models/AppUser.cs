using System.Security.Principal;
using Microsoft.AspNetCore.Identity;

namespace instagramClone.Entities.Models;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = null!;
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public string? WebsiteUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginDate { get; set; }

    // Account state flags
    public bool IsActive  { get; set; } = true;  // false = frozen
    public bool IsDeleted { get; set; } = false; // true  = permanently deleted
    public bool IsPrivate { get; set; } = false; // true  = private account

    // Navigation
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<StoryView>  StoryViews  { get; set; } = new List<StoryView>();
}