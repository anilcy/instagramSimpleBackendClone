namespace instagramClone.Entities.Dtos
{
    public class CreateCommentDto
    {
        public int PostId { get; set; }
        public string Content { get; set; }
    }
}