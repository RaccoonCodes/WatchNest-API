using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WatchNest.Constants;
using WatchNest.Models;

namespace WatchNest.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(Roles = RoleNames.Administrator)]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApiUsers> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedController(ApplicationDbContext context, UserManager<ApiUsers> userManager,
            RoleManager<IdentityRole> roleManager) =>
            (_context, _userManager, _roleManager) = (context, userManager, roleManager);

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Seeds existing Database",
            Description = "If the Database is empty, this will provide both roles and simple users " +
            "for both as a user and administrator"
        )]

        //T(x) = O(n) where n is number of series added to the record
        //O(1) when there is already data
        public async Task<IActionResult> SeedData()
        {
            try
            {
                if(await _context.Series.AnyAsync() || await _userManager.Users.AnyAsync())
                {
                    return Ok("Data already exist in the database, no action was made");
                } 

                if (_context.Database.GetPendingMigrations().Any())
                {
                    await _context.Database.MigrateAsync();
                }

                //seeding roles and users
                await EnsureRolesAsync();
                var testUser = await EnsureUserAsync("TestUser", "test-user@email.com", new[] { RoleNames.User });
                var testAdmin = await EnsureUserAsync("TestAdministrator", "test-admin@email.com", new[] { RoleNames.Administrator });

                //seeding data for user
                await SeedSeriesDataAsync(testUser);

                return Ok("Data seeded successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while seeding data: {ex.Message}");
            }

        }

        private async Task EnsureRolesAsync()
        {
            foreach (var roleName in new[] { RoleNames.Administrator, RoleNames.User })
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private async Task<ApiUsers> EnsureUserAsync(string userName, string email, string[] roles)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new ApiUsers
                {
                    UserName = userName,
                    Email = email,
                    NormalizedUserName = userName.ToUpper(),
                    NormalizedEmail = email.ToUpper()
                };
                var password = new PasswordHasher<ApiUsers>().HashPassword(user, "MyVeryOwnTestPassword123$");
                user.PasswordHash = password;
                await _userManager.CreateAsync(user);
            }

            await EnsureUserRolesAsync(user, roles);

            return user;
        }

        private async Task EnsureUserRolesAsync(ApiUsers user, string[] roles)
        {
            foreach (var roleName in roles)
            {
                if (!await _userManager.IsInRoleAsync(user, roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
            }
        }

        private async Task SeedSeriesDataAsync(ApiUsers user)
        {
            if (!_context.Series.Any())
            {
                _context.Series.AddRange
                (
                    new SeriesModel
                    {
                        UserID = user.Id,
                        TitleWatched = "Suzume",
                        SeasonWatched = 1,
                        Provider = "CrunchyRoll",
                        Genre = "Slice of Life"
                    },
                    new SeriesModel
                    {
                        UserID = user.Id,
                        TitleWatched = "The Great Cleric",
                        SeasonWatched = 1,
                        Provider = "CrunchyRoll",
                        Genre = "Fantasy"
                    }
                );
                await _context.SaveChangesAsync();
            }
        }
    }
}
