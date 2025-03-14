namespace instagramClone.Entities.Models;

public class Like
{
    public int Id { get; set; }
    public Guid UserId { get; set; }  // Beğeniyi yapan kullanıcı
    public int PostId { get; set; }   // Beğenilen gönderi
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   
    public AppUser User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}