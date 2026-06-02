namespace SocialMediaPlatform.Entities.Dtos.MessageDtos;

public class MessageCreateDto
{
    public Guid ReceiverId { get; set; }
    public string? Content { get; set; }
}