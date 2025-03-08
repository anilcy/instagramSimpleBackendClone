using instagramClone.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data;

public class InstagramDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public InstagramDbContext(DbContextOptions<InstagramDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)  // AppUser içerisinde Posts koleksiyonu tanımlı.
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }    
    public DbSet<Post> Posts { get; set; }
}