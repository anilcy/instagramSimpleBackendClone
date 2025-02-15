using instagramClone.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data;

public class InstagramDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public InstagramDbContext(DbContextOptions<InstagramDbContext> options) : base(options)
    {
    }

}