using LibraryRepository.Common;
using LibraryRestModels;
using LibraryService.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;

        public UserService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _config = config;
        }

        public async Task<string?> ValidateLoginAndGenerateTokenAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return null;

            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isValid) return null;

            var jwtSettings = _config.GetSection("Jwt");
            var keyString = jwtSettings["Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("FullName", user.FullName)
            };

            if (!int.TryParse(jwtSettings["ExpireMinutes"], out int expireMinutes))
            {
                throw new InvalidOperationException("JWT ExpireMinutes is not configured or invalid.");
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string?> RegisterUserAsync(string email, string password, string fullName)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
                return null;
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            await _userRepository.CreateUserAsync(email, passwordHash, fullName);
            return await ValidateLoginAndGenerateTokenAsync(email, password);
        }
    }
}
