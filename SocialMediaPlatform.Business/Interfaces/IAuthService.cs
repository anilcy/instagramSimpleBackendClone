using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Dtos.AuthDtos;

namespace SocialMediaPlatform.Business.Interfaces;

public interface IAuthService
{
    Task<AuthenticationResult> RegisterAsync(RegisterRequest request);
    Task<AuthenticationResult> LoginAsync(LoginRequest request);
}