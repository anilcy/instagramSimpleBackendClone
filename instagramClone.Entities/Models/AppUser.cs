using System.Security.Principal;
using Microsoft.AspNetCore.Identity;

namespace instagramClone.Entities.Models;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = null!;
    
    public string? ProfilePictureUrl { get; set; }
    
    public string? Bio { get; set; }
    
    public string? WebsiteUrl { get; set; } // Users blog or website
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastLoginDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsDeleted { get; set; } = false;
    

}