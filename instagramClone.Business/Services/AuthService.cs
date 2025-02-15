using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using instagramClone.Business.Interfaces;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;

namespace instagramClone.Business.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest request)
    {
        
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Bu email ile zaten kayıtlı bir kullanıcı mevcut." }
            };
        }

        
        var newUser = new AppUser
        {
            Email = request.Email,
            UserName = request.Username,
            FullName = request.FullName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdUser = await _userManager.CreateAsync(newUser, request.Password);

        if (!createdUser.Succeeded)
        {
            return new AuthenticationResult
            {
                Success = false,
                Errors = createdUser.Errors.Select(e => e.Description)
            };
        }

        

        // Token generation
        var token = GenerateJwtToken(newUser);

        return new AuthenticationResult
        {
            Success = true,
            Token = token
        };
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
    {
        // find user with email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Kullanıcı bulunamadı." }
            };
        }

        // password control
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return new AuthenticationResult
            {
                Success = false,
                Errors = new[] { "Geçersiz şifre." }
            };
        }

        // update last login
        user.LastLoginDate = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = GenerateJwtToken(user);

        return new AuthenticationResult
        {
            Success = true,
            Token = token
        };
    }

    private string GenerateJwtToken(AppUser user)
    {
        // jwt from .env
        var key = _configuration["JWT_KEY"];
        var issuer = _configuration["JWT_ISSUER"];
        var audience = _configuration["JWT_AUDIENCE"];
        var expireMinutes = Convert.ToDouble(_configuration["JWT_EXPIRE_MINUTES"]);

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
        {
            throw new ArgumentNullException("JWT settings are not configured properly.");
        }

        // SymmetricSecurityKey is generated using key
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expireMinutes);

        // claim for tokens
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("FullName", user.FullName)
        };

        // Token generated
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
