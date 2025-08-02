using instagramClone.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data;

public class InstagramDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public InstagramDbContext(DbContextOptions<InstagramDbContext> options) : base(options)
    {
    }
    public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<PostLike> Likes => Set<PostLike>();
        public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
        public DbSet<Follow> Follows => Set<Follow>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Story> Stories => Set<Story>();
        public DbSet<StoryView> StoryViews => Set<StoryView>();   

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global query filters
            modelBuilder.Entity<AppUser>()
                .HasQueryFilter(u => u.IsActive && !u.IsDeleted);

            modelBuilder.Entity<Post>()
                .HasQueryFilter(p => !p.IsDeleted && p.Author.IsActive && !p.Author.IsDeleted);
            modelBuilder.Entity<Comment>()
                .HasQueryFilter(c => !c.IsDeleted && c.Author.IsActive && !c.Author.IsDeleted);
            modelBuilder.Entity<PostLike>()
                .HasQueryFilter(pl => !pl.IsDeleted && pl.User.IsActive && !pl.User.IsDeleted);
            modelBuilder.Entity<CommentLike>()
                .HasQueryFilter(cl => !cl.IsDeleted && cl.User.IsActive && !cl.User.IsDeleted);
            modelBuilder.Entity<Follow>()
                .HasQueryFilter(f => !f.IsDeleted && f.Follower.IsActive && !f.Follower.IsDeleted && f.Followed.IsActive && !f.Followed.IsDeleted);
            modelBuilder.Entity<Message>()
                .HasQueryFilter(m => !m.IsDeleted && m.Sender.IsActive && !m.Sender.IsDeleted && m.Receiver.IsActive && !m.Receiver.IsDeleted);
            modelBuilder.Entity<Notification>()
                .HasQueryFilter(n => !n.IsDeleted && n.Recipient.IsActive && !n.Recipient.IsDeleted);
            modelBuilder.Entity<Story>()
                .HasQueryFilter(s => s.ExpiresAt > DateTime.UtcNow);  

            // Relationships and delete behaviors

            modelBuilder.Entity<Story>()
                .HasOne(s => s.User)
                .WithMany(u => u.Stories)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<StoryView>()
                .HasKey(v => new { v.UserId, v.StoryId });

            modelBuilder.Entity<StoryView>()
                .HasOne(v => v.Story)
                .WithMany(s => s.Views)
                .HasForeignKey(v => v.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StoryView>()
                .HasOne(v => v.User)
                .WithMany(u => u.StoryViews)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostLike>()
                .HasKey(pl => new { pl.UserId, pl.PostId });
            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommentLike>()
                .HasKey(cl => new { cl.UserId, cl.CommentId });
            modelBuilder.Entity<CommentLike>()
                .HasOne(cl => cl.User)
                .WithMany(u => u.CommentLikes)
                .HasForeignKey(cl => cl.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CommentLike>()
                .HasOne(cl => cl.Comment)
                .WithMany(c => c.Likes)
                .HasForeignKey(cl => cl.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowedId });
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Followed)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowedId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Recipient)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
}