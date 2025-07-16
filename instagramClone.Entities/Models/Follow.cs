namespace instagramClone.Entities.Models;

public enum FollowStatus { Pending, Accepted, Rejected }
public class Follow
{
    public Guid FollowerId { get; set; }
    public Guid FollowedId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DecidedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    public FollowStatus Status { get; set; } = FollowStatus.Pending;
    public AppUser Followed { get; set; } = null!;
    public AppUser Follower { get; set; } = null!;
}