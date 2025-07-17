namespace instagramClone.Entities.Dtos;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public required string UserName { get; set; }
    public required string FullName { get; set; }
    public string? ProfilePictureUrl { get; set; }
}