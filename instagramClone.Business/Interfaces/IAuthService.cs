using instagramClone.Entities.Dtos;

namespace instagramClone.Business.Interfaces;

public interface IAuthService
{
    Task<AuthenticationResult> RegisterAsync(RegisterRequest request);
    Task<AuthenticationResult> LoginAsync(LoginRequest request);
}