using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using instagramClone.Business.Interfaces;
using Microsoft.IdentityModel.Tokens;
using instagramClone.Entities.Models;
using DotNetEnv;

namespace instagramClone.Business.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly double _expireMinutes;

        public TokenService()
        {
            Env.Load();

            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new ArgumentNullException("JWT_KEY not found in .env")));
            _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new ArgumentNullException("JWT_ISSUER not found in .env");
            _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new ArgumentNullException("JWT_AUDIENCE not found in .env");
            _expireMinutes = Convert.ToDouble(Environment.GetEnvironmentVariable("JWT_EXPIRE_MINUTES") ?? "60");
        }

        public string GenerateJwtToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("FullName", user.FullName)
            };

            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_expireMinutes);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = _key
            };

            try
            {
                return tokenHandler.ValidateToken(token, validationParams, out _);
            }
            catch
            {
                return null; // Token geçersiz
            }
        }
    }
}