using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using instagramClone.Business.Interfaces;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;
using System.Threading.Tasks;

namespace instagramClone.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthService(UserManager<AppUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
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

            var token = _tokenService.GenerateJwtToken(newUser);

            return new AuthenticationResult
            {
                Success = true,
                Token = token
            };
        }

        public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Errors = new[] { "Kullanıcı bulunamadı." }
                };
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Errors = new[] { "Geçersiz şifre." }
                };
            }

            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var token = _tokenService.GenerateJwtToken(user);

            return new AuthenticationResult
            {
                Success = true,
                Token = token
            };
        }
    }
}
