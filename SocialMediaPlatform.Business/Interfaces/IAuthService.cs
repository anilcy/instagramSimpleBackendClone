using SocialMediaPlatform.Entities.Dtos;

namespace SocialMediaPlatform.Business.Interfaces;

public interface IAuthService
{
    Task<AuthenticationResult> RegisterAsync(RegisterRequest request);
    Task<AuthenticationResult> LoginAsync(LoginRequest request);
}