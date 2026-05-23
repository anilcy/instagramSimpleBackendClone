namespace SocialMediaPlatform.Business.Options;

public sealed class JwtOptions
{
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public double ExpireMinutes { get; init; } = 60;
}
