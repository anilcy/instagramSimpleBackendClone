namespace instagramClone.Entities.Dtos;

public class RegisterRequest
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Password { get; set; }
}