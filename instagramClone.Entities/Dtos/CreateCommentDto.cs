namespace instagramClone.Entities.Dtos
{
    public class CreateCommentDto
    {
        public int PostId { get; set; }
        public required string Content { get; set; }
        public int? ParentCommentId { get; set; }
    }
}