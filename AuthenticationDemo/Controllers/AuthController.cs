using AuthenticationDemo.Entities;
using AuthenticationDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IConfiguration configuration) : ControllerBase
    {
        public static User user = new();
        [HttpPost("Register")]
        public ActionResult<User> Rergister(UserDto userDto)
        {
            var hashPassword = new PasswordHasher<User>().HashPassword(user, userDto.Password);

            user.UserName = userDto.UserName;
            user.PasswordHash = hashPassword;

            return Ok(user);
        }

        [HttpPost("Login")]
        public ActionResult<string> Login(UserDto userDto)
        {
            if (user.UserName != userDto.UserName)
            {
                return BadRequest("Not Found");
            }
            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, userDto.Password)
                == PasswordVerificationResult.Failed)
            {
                return BadRequest("Wrong");
            }

            string token = CreateToken(user);
            return Ok(token);
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.UserName)
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
