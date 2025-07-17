namespace instagramClone.Entities.Dtos
{
    public class CreateCommentDto
    {
        public int PostId { get; set; }
        public required string Content { get; set; }
    }
}