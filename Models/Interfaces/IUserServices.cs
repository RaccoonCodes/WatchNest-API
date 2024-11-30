using Microsoft.AspNetCore.Identity;
using WatchNest.DTO;

namespace WatchNest.Models.Interfaces
{
    public interface IUserServices
    {
        Task<IdentityResult> RegisterAsync(RegisterDTO input);
        Task<string?> LoginAsync(LoginDTO input);
    }
}
