using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WatchNest.Constants;
using WatchNest.DTO;
using WatchNest.Models.Interfaces;

namespace WatchNest.Models.Implementation
{
    public class UserService : IUserServices
    {
        private readonly UserManager<ApiUsers> _usersManager;
        private readonly IConfiguration _configuration;
        //private readonly IDistributedCache _cache;

        public UserService(UserManager<ApiUsers> usersManager, IConfiguration configuration)
        => (_usersManager, _configuration) = (usersManager, configuration);

        //T(x) = O(1)
        public async Task<IdentityResult> RegisterAsync(RegisterDTO input)
        {
            var newUser = new ApiUsers{
                UserName = input.UserName,
                Email = input.Email
            };

            var result = await _usersManager.CreateAsync(newUser, input.Password);

            if (result.Succeeded)
            {
                await _usersManager.AddToRoleAsync(newUser,RoleNames.User);
            }

            return result;
        }

        //T(x) = O(n) where n is the number of roles assign to the user
        public async Task<string?> LoginAsync(LoginDTO input)
        {
           var user = await _usersManager.FindByNameAsync(input.UserName);
            
            //checking username and password
            if(user ==null || !await _usersManager.CheckPasswordAsync(user, input.Password))
            {
                return null; //Invalid attempt
            }

            var signingKey = _configuration["JWT:SigningKey"];
            if (string.IsNullOrEmpty(signingKey))
            {
                throw new InvalidOperationException("JWT signing key is not configured.");
            }

            //Generating signing credentials when both username and password Match
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(signingKey)),SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
               new Claim (ClaimTypes.Name,user.UserName),
               new Claim (ClaimTypes.NameIdentifier, user.Id)
            };

            claims.AddRange((await _usersManager.GetRolesAsync(user))
                .Select(c => new Claim(ClaimTypes.Role, c)));

            //Instantiates JWT object instance
            var jwtObject = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddSeconds(600), //valid for about 10 min
                signingCredentials: signingCredentials
            );

            //return JWT encrypted string
            return new JwtSecurityTokenHandler().WriteToken(jwtObject);
        }
    }
}
