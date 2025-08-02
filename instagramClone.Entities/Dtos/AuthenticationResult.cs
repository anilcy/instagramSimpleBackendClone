public class AuthenticationResult
{
    public bool Success { get; set; }
    public required IEnumerable<string> Errors { get; set; }
    public string? Token { get; set; }
    // Optional: public string RefreshToken { get; set; }
    public Guid   Id                { get; set; }
    public string UserName          { get; set; } = null!;
    public string FullName          { get; set; } = null!;
    public string? ProfilePictureUrl{ get; set; }
}