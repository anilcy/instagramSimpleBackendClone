namespace SocialMediaPlatform.Entities.Dtos.CommentDtos;

    public class CommentCreateDto
    {
        public Guid PostId { get; set; }
        public required string Content { get; set; }
        public Guid? ParentCommentId { get; set; }
    }