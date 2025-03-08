using System.Security.Claims;
using instagramClone.Entities.Models;

namespace instagramClone.Business.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(AppUser user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}