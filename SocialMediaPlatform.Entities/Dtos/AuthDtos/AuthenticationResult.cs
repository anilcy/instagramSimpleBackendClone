namespace SocialMediaPlatform.Entities.Dtos.AuthDtos;
public class AuthenticationResult
{
    public bool Success { get; set; }
    public required IEnumerable<string> Errors { get; set; }
    public string? Token { get; set; }
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string? FullName { get; set; }
    public string? ProfilePictureUrl { get; set; }
}