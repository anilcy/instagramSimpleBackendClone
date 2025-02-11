using instagramClone.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data;

public class DbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public DbContext(DbContextOptions<DbContext> options) : base(options)
    {
        
        
    }
}