using System;

namespace instagramClone.Entities.Models;

public class StoryView
{
    public int Id { get; set; }

    public int StoryId { get; set; }
    public Story Story { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}