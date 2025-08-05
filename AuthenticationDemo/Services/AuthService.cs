using AuthenticationDemo.Data;
using AuthenticationDemo.Entities;
using AuthenticationDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthenticationDemo.Services
{
    public class AuthService(UserDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<TokenResponseDto?> LoginAsync(UserDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if (user == null)
            {
                return null;
            }
            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshToken(user),
            };
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            if (await context.Users.AnyAsync(u => u.UserName == request.UserName))
            {
                return null;
            }

            var user = new User();
            var hashPassword = new PasswordHasher<User>().HashPassword(user, request.Password);

            user.UserName = request.UserName;
            user.PasswordHash = hashPassword;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto requestDto)
        {
            var user = await VAlidateRefreshTokenAsync(requestDto.UserId, requestDto.RefreshToken);
            if (user is null)
                return null;

            return await CreateTokenResponse(user);
        }

        private async Task<User> VAlidateRefreshTokenAsync(Guid userId,string refreshToken)
        {
            var user = await context.Users.FindAsync(userId);
            if(user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpireTime <= DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }

        private string GenericRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshToken(User user)
        {
            var refreshToken = GenericRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(1);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("JWT:Token")!));

            var cards = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("JWT:Issuer"),
                audience: configuration.GetValue<string>("JWT:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: cards
             );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
