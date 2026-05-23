using System.Security.Claims;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Business.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(AppUser user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}