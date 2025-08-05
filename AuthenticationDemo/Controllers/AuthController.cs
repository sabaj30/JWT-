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
        public async Task<ActionResult<string>> Login(UserDto userDto)
        {
            var token = await authService.LoginAsync(userDto);
            if (token == null)
            {
                return BadRequest();
            }

            return Ok(token);
        }


        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("hi");
        }
    }
}
