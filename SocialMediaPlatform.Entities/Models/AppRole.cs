using Microsoft.AspNetCore.Identity;

namespace SocialMediaPlatform.Entities.Models;

public class AppRole : IdentityRole<Guid>
{
    public required string RoleName { get; set; }
}