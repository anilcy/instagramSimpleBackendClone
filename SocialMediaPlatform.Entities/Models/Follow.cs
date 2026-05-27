using System;

namespace SocialMediaPlatform.Entities.Models;

public enum FollowStatus { Pending, Accepted, Rejected }
public class Follow
{
    public Follow(Guid followerId, Guid followedId, bool isPrivate)
    {
        if (followerId == Guid.Empty)
            throw new ArgumentException("FollowerId cannot be empty.", nameof(followerId));

        if (followedId == Guid.Empty)
            throw new ArgumentException("FollowedId cannot be empty.", nameof(followedId));
        
        if (followerId == followedId)
            throw new ArgumentException("Users cannot follow themselves.");

        Status = isPrivate ? FollowStatus.Pending : FollowStatus.Accepted;
        DecidedAt = isPrivate ? null : DateTimeOffset.UtcNow;
        FollowedId = followedId;
        FollowerId = followerId;
        CreatedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }
    private Follow() { }
    public Guid FollowerId { get; private set; }
    public Guid FollowedId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } 
    public DateTimeOffset? DecidedAt { get; private set; }
    //public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; } 
    
    public FollowStatus Status { get; private set; } 
    
    public AppUser Followed { get; private set; } = null!;
    public AppUser Follower { get; private set; } = null!;
    
    public void AcceptRequest()
    {
        if (Status != FollowStatus.Pending)
            throw new InvalidOperationException("Only pending follow requests can be accepted.");
        Status = FollowStatus.Accepted;
        DecidedAt = DateTimeOffset.UtcNow;
    }
    
    public void RejectRequest()
    {
        if (Status != FollowStatus.Pending)
            throw new InvalidOperationException("Only pending follow requests can be rejected.");
        Status = FollowStatus.Rejected;
        DecidedAt = DateTimeOffset.UtcNow;
    }
    
    public void SoftDeleteFollow()
    {
        if (IsDeleted)
            return;
        IsDeleted = true;
        //UpdatedAt = DateTimeOffset.UtcNow;
    }
}