namespace instagramClone.Entities.Models;

    public class Story
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }         
        public AppUser User { get; set; } = null!;

        public string MediaUrl { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }   // = CreatedAt + 24h (servis set edecek)

        public ICollection<StoryView> Views { get; set; } = new List<StoryView>();
    }

