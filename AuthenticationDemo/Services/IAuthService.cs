using AuthenticationDemo.Entities;
using AuthenticationDemo.Models;

namespace AuthenticationDemo.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<string?> LoginAsync(UserDto request);
    }
}
