namespace instagramClone.Entities.Dtos;

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string FullName { get; set; }
    public required string Password { get; set; }
}