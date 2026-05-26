using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SocialMediaPlatform.Entities.Models;

public class AppUser : IdentityUser<Guid>
{
    public AppUser(string userName, string email, string? fullName)
    {
        UserName = string.IsNullOrWhiteSpace(userName)
            ? throw new ArgumentException("Username cannot be empty.", nameof(userName))
            : userName;
        Email = string.IsNullOrWhiteSpace(email)
            ? throw new ArgumentException("Email cannot be empty", nameof(email))
            : email;
        FullName = fullName;
        CreatedAt = DateTimeOffset.UtcNow;
        LastLoginDate = DateTimeOffset.UtcNow;
        IsActive = true;
        IsDeleted = false;
        IsPrivate = false;
    }
    
    private AppUser() { }
    public string? FullName { get; private set; } 
    public string? ProfilePictureUrl { get; private set; }
    public string? Bio { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } 
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset LastLoginDate { get; private set; } 

    // Account state flags
    public bool IsActive  { get; private set; } 
    public bool IsDeleted { get; private set; } 
    public bool IsPrivate { get; private set; } 

    // Navigation
    public ICollection<Post> Posts { get; private set; } = new List<Post>();
    public ICollection<Comment> Comments { get;private set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; private set; } = new List<PostLike>();
    public ICollection<CommentLike> CommentLikes { get; private set; } = new List<CommentLike>();
    public ICollection<Follow> Followers { get; private set; } = new List<Follow>();
    public ICollection<Follow> Following { get; private set; } = new List<Follow>();
    public ICollection<Message> SentMessages { get; private set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; private set; } = new List<Message>();
    public ICollection<Notification> Notifications { get; private set; } = new List<Notification>();
    public ICollection<Story> Stories { get; private set; } = new List<Story>();
    public ICollection<StoryView>  StoryViews  { get; private set; } = new List<StoryView>();
    
    
    public void UpdateProfile(string? fullName, string? profilePictureUrl, string? bio, string? websiteUrl)
    {
        FullName = fullName;
        ProfilePictureUrl = profilePictureUrl;
        Bio = bio;
        WebsiteUrl = websiteUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void SetPrivate(bool isPrivate)
    {
        IsPrivate = isPrivate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void DeactivateAccount()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void ActivateAccount()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDeleteAccount()
    {
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void UpdateLastLoginDate()
    {
        LastLoginDate = DateTimeOffset.UtcNow;
    }
    
}