namespace instagramClone.Entities.Dtos;

public class PostDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }
    public string Caption { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; }
}