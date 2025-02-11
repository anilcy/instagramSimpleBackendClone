using Microsoft.AspNetCore.Identity;

namespace instagramClone.Entities.Models;

public class AppRole : IdentityRole<Guid>
{
    public string RoleName { get; set; }
}