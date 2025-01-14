using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using WatchNestApp.Models.Interface;
using System.IdentityModel.Tokens.Jwt;
using WatchNestApp.Constants;
using System.Security.Claims;


namespace WatchNestApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAccountService _accountService;

        public HomeController(ILogger<HomeController> logger, IAccountService accountService) =>
            (_logger,_accountService) = (logger, accountService);

        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (HttpContext.User.IsInRole(RoleNames.Administrator))
                    return RedirectToAction(nameof(Index), "Admin");
                else
                     return RedirectToAction("Index", "WatchList");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Index));
            }
                var (isSuccess, errorMessage, jwtCookie) = await _accountService.LoginAsync(username, password);

            if (!isSuccess)
            {
                ModelState.AddModelError("", errorMessage!);
                return View(nameof(Index));
            }

            if (!string.IsNullOrEmpty(jwtCookie))
            {
                Response.Headers.Append("Set-Cookie", jwtCookie);

            }
            return RedirectToAction(nameof(RouteTo));
           
        }

        public IActionResult RouteTo()
        {
            var user = HttpContext.User;
            if (user.IsInRole(RoleNames.Administrator))
            {
                return RedirectToAction(nameof(Index), "Admin");

            }
            else
                return RedirectToAction(nameof(Index), "WatchList");
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string email)
        {

            if (!ModelState.IsValid)
            {
                return View(nameof(Register));
            }

            var (isSuccess, errorMessage) = await _accountService.RegisterAsync(username, password, email);

            if (!isSuccess)
            {
                ModelState.AddModelError("", errorMessage!);
                return View(nameof(Register));
            }

            if (HttpContext.User.IsInRole(RoleNames.Administrator))
            {
                TempData["ImportantMessage"] = $"New User Registration Complete for {username}!";
                return RedirectToAction(nameof(Index), "Admin");
            }

            TempData["ImportantMessage"] = "Registration complete! You can now log in!";
            return RedirectToAction(nameof(Index), "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var isSuccess = await _accountService.LogoutAsync();

                if (isSuccess)
                {
                    _logger.LogInformation("User successfully logged out.");
                    TempData["ImportantMessage"] = "You have successfully logged out!";
                }
                else
                {
                    _logger.LogWarning("Logout failed. The account service returned a failure response.");
                    TempData["ImportantMessage"] = "Logout failed! Forced logged out & contact support!";
                }
            }
            catch (Exception)
            {
                _logger.LogError("API is down, please reboot or restart it!");
                TempData["ImportantMessage"] = "API is down! Please contact support.";
            }

            await HttpContext.SignOutAsync();
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction(nameof(Index), "Home");

        }

        public IActionResult AccessDenied() => View();
        public IActionResult Error() => View();
        public IActionResult ErrorCode(int code)
        {
            if (code == 404)
            {
                return View("NotFound");
            }
            return View("Error");
        }

    }
}
