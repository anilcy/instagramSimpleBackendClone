public class AuthenticationResult
{
    public bool Success { get; set; }
    public IEnumerable<string> Errors { get; set; }
    public string Token { get; set; }
    // Optional: public string RefreshToken { get; set; }
}