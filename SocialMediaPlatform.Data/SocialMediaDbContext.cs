using System;
using SocialMediaPlatform.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaPlatform.Data;

public class SocialMediaDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public SocialMediaDbContext(DbContextOptions<SocialMediaDbContext> options) : base(options) { }
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostLike> Likes => Set<PostLike>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<StoryLike> StoryLikes => Set<StoryLike>();
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
                .HasQueryFilter(s => !s.IsDeleted&& s.User.IsActive && !s.User.IsDeleted && s.ExpiresAt > DateTimeOffset.UtcNow);  
            modelBuilder.Entity<StoryLike>()
                .HasQueryFilter(sl => !sl.IsDeleted && sl.User.IsActive && !sl.User.IsDeleted && sl.Story.ExpiresAt > DateTimeOffset.UtcNow);
            modelBuilder.Entity<StoryView>()
                .HasQueryFilter(sv => sv.Story.ExpiresAt > DateTimeOffset.UtcNow && sv.User.IsActive && !sv.User.IsDeleted);

            // Relationships and delete behaviors

            modelBuilder.Entity<Story>()
                .HasOne(s => s.User)
                .WithMany(u => u.Stories)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Story>()
                .HasMany(s => s.Likes)
                .WithOne(sl => sl.Story)
                .HasForeignKey(sl => sl.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Story>()
                .HasMany(s => s.Views)
                .WithOne(sv => sv.Story)
                .HasForeignKey(sv => sv.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
            
            
            modelBuilder.Entity<StoryLike>()
                .HasKey(sl => new { sl.UserId, sl.StoryId });
            modelBuilder.Entity<StoryLike>()
                .HasOne(sl => sl.User)
                .WithMany(u => u.StoryLikes)
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            
            modelBuilder.Entity<StoryView>()
                .HasKey(v => new { v.UserId, v.StoryId });
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
            modelBuilder.Entity<Post>()
                .HasMany(p => p.Likes)
                .WithOne(pl => pl.Post)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            
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
            modelBuilder.Entity<Comment>()
                .HasMany(c => c.Likes)
                .WithOne(cl => cl.Comment)
                .HasForeignKey(cl => cl.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<PostLike>()
                .HasKey(pl => new { pl.UserId, pl.PostId });
            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<CommentLike>()
                .HasKey(cl => new { cl.UserId, cl.CommentId });
            modelBuilder.Entity<CommentLike>()
                .HasOne(cl => cl.User)
                .WithMany(u => u.CommentLikes)
                .HasForeignKey(cl => cl.UserId)
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
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Actor)
                // WE Don't USE NAVIGATION PROPERTY SINCE BOTH ACTOR AND RECIPIENT BELONG TO APPUSER, IT WILL
                // CAUSE AMBIGUITY, AND WE WILL NEVER BE USING "TRIGGERED NOTIFICATIONS" BY A USER SO NO NOTIFICATIONS 
                // HERE FOR ACTOR. OPTION 2 IS HAVING RECEIVED AND TRIGGERED NOTIFICATIONS IN APPUSER, BUT IT'S NOT NECESSARY
                // , WE CAN JUST QUERY NOTIFICATIONS TABLE TO GET ALL NOTIFICATIONS RELATED TO A USER
                .WithMany()                    
                .HasForeignKey(n => n.ActorId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Post)
                .WithMany()  // no notifications here because we won't use post.Notifications
                .HasForeignKey(n => n.PostId)
                .OnDelete(DeleteBehavior.SetNull); 
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Comment)
                .WithMany() // no notifications here because we won't use post.Notifications
                .HasForeignKey(n => n.CommentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
}