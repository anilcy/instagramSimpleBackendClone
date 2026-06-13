using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Models;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Dtos.AuthDtos;

namespace SocialMediaPlatform.Business.Services
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
                    Errors = new[] { "Invalid email or password." }
                };
            }
            
            var existingUsername = await _userManager.FindByNameAsync(request.Username);
            if (existingUsername != null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Errors = new[] { "This username is already taken." }
                };
            }
            
            var newUser = new AppUser(request.Username, request.Email, request.FullName);

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
                Token = token,
                Errors = Array.Empty<string>(),
                Id = newUser.Id,
                UserName = newUser.UserName!,
                FullName = newUser.FullName,
                ProfilePictureUrl = newUser.ProfilePictureUrl
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
                    Errors = new[] { "User not found." }
                };
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Errors = new[] { "Invalid email or password." }
                };
            };

            var token = _tokenService.GenerateJwtToken(user);
            user.UpdateLastLoginDate();
            await _userManager.UpdateAsync(user);

            return new AuthenticationResult
            {
                Success = true,
                Token = token,
                Errors = Array.Empty<string>(),
                Id                  = user.Id,
                UserName            = user.UserName!,
                FullName            = user.FullName,
                ProfilePictureUrl   = user.ProfilePictureUrl
            };
        }
    }
}
