using AuthenticationDemo.Entities;
using AuthenticationDemo.Models;
using AuthenticationDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        public static User user = new();
        [HttpPost("Register")]
        public async Task<ActionResult<User>> Rergister(UserDto userDto)
        {
            var user = await authService.RegisterAsync(userDto);
            if (user == null)
            {
                return BadRequest();
            }

            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserDto userDto)
        {
            var result = await authService.LoginAsync(userDto);
            if (result == null)
            {
                return BadRequest();
            }

            return Ok(result);
        }

        [HttpPost("Refresh-Token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto requestDto)
        {
            var result = await authService.RefreshTokenAsync(requestDto);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized("refresh token invalid");

            return Ok(result);

        }


        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("hi");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Admin Only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("Admin");
        }
    }
}
