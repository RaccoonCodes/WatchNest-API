using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using WatchNestApp.Constants;
using WatchNestApp.DTO;
using WatchNestApp.Extensions;
using WatchNestApp.Models;
using WatchNestApp.Models.Interface;

namespace WatchNestApp.Controllers
{
    [Authorize(Roles = RoleNames.Administrator)]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IAdminService _adminService;

        public AdminController(ILogger<AdminController> logger, IAdminService adminService)
            => ( _logger, _adminService) = (logger, adminService);

        public async Task<IActionResult> Index()
        {
            try
            {
                var apiResponse = await _adminService.GetAllUsersAsync();

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to deserialize the API response.");
                    return View();
                }
                ViewBag.Pagination = apiResponse;
                var userList = apiResponse?.Data;
                return View(userList);

            }
            catch (HttpRequestException)
            {
                _logger.LogError("The API for WatchNest is Down. Please ensure it is running or reboot API connection!");
                TempData["ImportantMessage"] = "Error - API is down, Contact Support!";
                return View("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unknown error just occurred! Message: {ex.Message}");
                TempData["ImportantMessage"] = "An unexpected error occurred. Please contact support.";
                return View("Index", "Home");
            }
        }

        public async Task<IActionResult> AdminGetSeries()
        {
            try
            {

                var seriesResponse = await _adminService.GetAllSeriesAsync();
                if (seriesResponse == null || seriesResponse.Data == null)
                {
                    TempData["ErrorMessage"] = "Failed to load series.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Pagination = seriesResponse;
                return View(seriesResponse.Data);
            }
            catch (HttpRequestException)
            {
                _logger.LogError("The API for WatchNest is Down. Please ensure it is running or reboot API connection!");
                TempData["ImportantMessage"] = "Error - API is down, Contact Support!";
                return View("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unknown error just occurred! Message: {ex.Message}");
                TempData["ImportantMessage"] = "An unexpected error occurred. Please contact support.";
                return View("Index", "Home");
            }
        }

        public async Task<IActionResult> LookUserSeries(FilterModel filterModel)
        {

            var cacheKey = "PaginationKeys".GenerateCacheKey();
            var flag = HttpContext.Session.GetString("FlagUserRefreshList");

            if (flag == "true")
            {
                await _adminService.RefreshCacheAsync(cacheKey);
                HttpContext.Session.SetString("FlagUserRefreshList", "false");
            }

            ApiResponse<UserModel> userList;
            try
            {
                userList = await _adminService.GetCachedUserListAsync(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user list.");
                return RedirectToAction(nameof(Index), "Admin");
            }

            ViewBag.UserList = userList;

            if (string.IsNullOrEmpty(filterModel.UserID))
            {
                return View((Enumerable.Empty<SeriesDTO>(), filterModel));
            }

            var filterDTO = new FilterDTO
            (
                filterModel.SortColumn,
                filterModel.SortOrder,
                filterModel.FilterQuery,
                filterModel.UserID
            );

            ApiResponse<SeriesDTO> seriesList;
            try
            {
                seriesList = await _adminService.GetFilteredSeriesAsync(filterDTO);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered series.");
                return RedirectToAction(nameof(Index), "Admin");
            }

            ViewBag.Pagination = seriesList;

            return View((seriesList.Data ?? Enumerable.Empty<SeriesDTO>(), filterModel));

        }

        public IActionResult DeleteUserView(string userID, string username)
        {
            var AdminID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == AdminID)
            {
                TempData["ImportantMessage"] = "Error - You cannot delete your own Credentials";
                return RedirectToAction(nameof(Index), "Admin");
            }


            return View((userID, username));

        }
        [HttpPost]
        public async Task<IActionResult> DeleteUserPost(string userID)
        {
            var response = await _adminService.DeleteUserAsync(userID);
            if (!response)
            {
                TempData["ImportantMessage"] = "Error: Cannot Delete user, Contact Support!";
            }
            else
            {
                TempData["ImportantMessage"] = "Successfully deleted selected user!";
                HttpContext.Session.SetString("FlagUserRefreshList", "true");
            }

            return RedirectToAction(nameof(Index), "Admin");

        }

        [HttpPost]
        public IActionResult PartialTableUpdate([FromBody] IEnumerable<SeriesDTO> data)
        {
            if (data == null || !data.Any())
            {
                return PartialView("_AdminSeriesTablePartial", new List<SeriesDTO>());
            }

            return PartialView("_AdminSeriesTablePartial", data);
        }

        public IActionResult PartialUserTable([FromBody] IEnumerable<UserModel> data)
        {
            if (data == null || !data.Any())
            {
                return PartialView("_AdminUsersTablePartial", new List<SeriesDTO>());
            }

            return PartialView("_AdminUsersTablePartial", data);
        }

        [HttpPost]
        public IActionResult PaginationUpdate([FromBody] ApiResponse<UserModel> apiResponse)
        {
            if (apiResponse == null || apiResponse.Links == null || !apiResponse.Links.Any())
            {
                return PartialView("_PaginationUserPartial", new ApiResponse<UserModel>());
            }

            return PartialView("_PaginationUserPartial", apiResponse);
        }

    }
}
