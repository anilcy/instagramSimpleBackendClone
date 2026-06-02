namespace SocialMediaPlatform.Entities.Dtos.AuthDtos;

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Username { get; set; }
    public  string? FullName { get; set; }
    public required string Password { get; set; }
}