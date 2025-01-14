using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WatchNestApp.DTO;
using WatchNestApp.Models;
using WatchNestApp.Models.Interface;
using Microsoft.AspNetCore.Authorization;
using WatchNestApp.Constants;

namespace WatchNestApp.Controllers
{
    [Authorize(Roles = $"{RoleNames.User},{RoleNames.Administrator}")]
    public class WatchListController : Controller
    {
        private readonly ILogger<WatchListController> _logger;
        private readonly IWatchListService _watchListService;

        public WatchListController(ILogger<WatchListController> logger, IWatchListService watchListService)
            => (_logger, _watchListService) = (logger, watchListService);
        public async Task<IActionResult> Index(bool forceRefresh = false)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home");
            }
            try
            {

                var apiResponse = await _watchListService.GetSeriesAsync(userId, forceRefresh);

                if (apiResponse == null)
                {
                    await HttpContext.SignOutAsync();
                    Response.Cookies.Delete("AuthToken");
                    TempData["ImportantMessage"] = "Your session was expired, please log in again!";
                    return RedirectToAction("Index", "Home");
                }

                var seriesList = apiResponse?.Data;
                return View(seriesList);
            }
            catch (HttpRequestException)
            {
                _logger.LogError("The API for WatchNest is Down. Please ensure it is running or reboot API connection!");
                TempData["ImportantMessage"] = "Error: API is down, Please try again later!";
                return View(nameof(AddSeries));
            }
            catch (Exception)
            {
                TempData["ImportantMessage"] = "An unexpected error occurred. Please contact support.";
                return View(nameof(AddSeries));
            }

        }

        public IActionResult AddSeries() => View();

        [HttpPost]
        public async Task<IActionResult> AddSeries(NewSeriesModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(AddSeries));
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var newSeriesDTO = new NewSeriesDTO
                (userId!, model.TitleWatched, model.SeasonWatched, model.ProviderWatched, model.Genre);


            try
            {

                var response = await _watchListService.AddSeriesAsync(newSeriesDTO);

                if (response == true)
                {
                    TempData["ImportantMessage"] = $"{model.TitleWatched} created and added to your list!";
                    return RedirectToAction(nameof(Index), "WatchList", new { forceRefresh = true });

                }
                else
                {
                    TempData["ImportantMessage"] = "Failed to add series! please contact support!";
                    return View(nameof(AddSeries));
                }

            }
            catch (HttpRequestException)
            {
                _logger.LogError("The API for WatchNest is Down. Please ensure it is running or reboot API connection!");
                TempData["ImportantMessage"] = "Error: API is down, Please try again later!";
                return View(nameof(AddSeries));
            }
            catch (Exception)
            {
                TempData["ImportantMessage"] = "An unexpected error occurred. Please contact support.";
                return View(nameof(AddSeries));
            }
        }

        public async Task<IActionResult> EditView(int infoID)
        {
            try
            {
                var series = await _watchListService.GetSeriesByIdAsync(infoID);

                if (series == null)
                {
                    TempData["ImportantMessage"] = "There was an error with selected series, Please contact support!";
                    return RedirectToAction(nameof(Index), "WatchList");
                }

                return View(series);
            }
            catch (HttpRequestException)
            {
                _logger.LogError("The API for WatchNest is Down. Please ensure it is running or reboot API connection!");
                TempData["ImportantMessage"] = "Error: API is down, Please try again later!";
                return RedirectToAction(nameof(Index), "WatchList");
            }
            catch (Exception)
            {
                TempData["ImportantMessage"] = "An unexpected error occurred. Please contact support!";
                return RedirectToAction(nameof(Index), "WatchList");
            }

        }

        [HttpPost]
        public async Task<IActionResult> EditView(UpdateModel updateModel)
        {
            if (!ModelState.IsValid)
            {
                return View(updateModel);
            }
            
            var updateDTO = new UpdateSeriesDTO(updateModel.TitleWatched, updateModel.SeasonWatched,
              updateModel.Provider, updateModel.Genre, updateModel.SeriesID, updateModel.UserID);

            try
            {


                var response = await _watchListService.UpdateSeriesAsync(updateDTO);

                if (response == true)
                {
                    TempData["ImportantMessage"] = $"{updateModel.TitleWatched} Updated! ";
                    
                    if (HttpContext.User.IsInRole(RoleNames.Administrator))
                    {
                        return RedirectToAction("LookUserSeries", "Admin");

                    }

                    return RedirectToAction(nameof(Index), "WatchList", new { forceRefresh = true });
                }
                TempData["ImportantMessage"] = "There was an error with selected series, Please contact support!";
                return RedirectToAction(nameof(EditView), updateModel.SeriesID);
            }

            catch (HttpRequestException)
            {
                _logger.LogError("The API for WatchNest is Down. Please ensure it is running or reboot API connection!");
                TempData["ImportantMessage"] = "Error: API is down, Please try again later!";
                return RedirectToAction(nameof(Index), "WatchList");
            }
            catch (Exception)
            {
                TempData["ImportantMessage"] = "An unexpected error occurred. Please contact support!";
                return RedirectToAction(nameof(Index), "WatchList");
            }
        }
        public async Task<IActionResult> DeleteView(int infoID)
        {
            try
            {
                var response = await _watchListService.GetSeriesByIdAsync(infoID);

                if (response == null)
                {
                    TempData["ImportantMessage"] = "There was an error with selected series, Please contact support!";
                    return RedirectToAction(nameof(Index), "WatchList");
                }
                var series = new DeleteModel
                {
                    TitleWatched = response.TitleWatched,
                    SeasonWatched = response.SeasonWatched,
                    Provider = response.Provider,
                    Genre = response.Genre,
                    SeriesID = response.SeriesID,
                    UserID = response.UserID,
                };

                return View(series);
            }
            catch (HttpRequestException)
            {
                _logger.LogError("The API for WatchNest is Down. Please ensure it is running or reboot API connection!");
                TempData["ImportantMessage"] = "Error: API is down, Please try again later!";
                return RedirectToAction(nameof(Index), "WatchList");
            }
            catch (Exception)
            {
                TempData["ImportantMessage"] = "An unexpected error occurred. Please contact support!";
                return RedirectToAction(nameof(Index), "WatchList");
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int infoID)
        {
            try
            {
                var response = await _watchListService.DeleteSeriesAsync(infoID);

                if (response == true)
                {
                    TempData["ImportantMessage"] = "Selected series successfully deleted!";
                }
                else
                {
                    TempData["ImportantMessage"] = "There was an error with Deleting series, Please contact support!";
                }
                if (HttpContext.User.IsInRole(RoleNames.Administrator))
                {
                    return RedirectToAction("LookUserSeries", "Admin");

                }

                return RedirectToAction(nameof(Index), "WatchList", new { forceRefresh = true });
            }
            catch (HttpRequestException)
            {
                _logger.LogError("The API for WatchNest is Down. Please ensure it is running or reboot API connection!");
                TempData["ImportantMessage"] = "Error: API is down, Please try again later!";
                return RedirectToAction(nameof(Index), "WatchList");
            }
            catch (Exception)
            {
                TempData["ImportantMessage"] = "An unexpected error occurred. Please contact support!";
                return RedirectToAction(nameof(Index), "WatchList");
            }
        }

        public async Task<IActionResult> Filter(FilterModel filterModel)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home");
            }

            var filterDTO = new FilterDTO(filterModel.SortColumn, filterModel.SortOrder, filterModel.FilterQuery, userId);

            try
            {
                var response = await _watchListService.FilterSeriesAsync(filterDTO);

                if (response == null)
                {
                    TempData["ImportantMessage"] = "Error: Unable to retrieve data.";
                    return RedirectToAction("Index", "WatchList");
                }

                var seriesList = response.Data;
                ViewBag.Pagination = response;
                return View(((IEnumerable<SeriesDTO>)seriesList!, filterModel));

            }
            catch (HttpRequestException)
            {
                _logger.LogError("The API for WatchNest is Down. Please ensure it is running or reboot API connection!");
                TempData["ImportantMessage"] = "Error: API is down, Please try again later!";
                return RedirectToAction(nameof(Index), "WatchList");
            }
            catch (Exception)
            {
                TempData["ImportantMessage"] = "An unexpected error occurred. Please contact support!";
                return RedirectToAction(nameof(Index), "WatchList");
            }

        }

        [HttpPost]
        public IActionResult PartialTableUpdate([FromBody] IEnumerable<SeriesDTO> data)
        {
            if (data == null || !data.Any())
            {
                return PartialView("_SeriesTablePartial", new List<SeriesDTO>());
            }

            return PartialView("_SeriesTablePartial", data);
        }
        [HttpPost]
        public IActionResult PaginationUpdate([FromBody] ApiResponse<SeriesDTO> apiResponse)
        {
            if (apiResponse == null || apiResponse.Links == null || !apiResponse.Links.Any())
            {
                return PartialView("_PaginationPartial", new ApiResponse<SeriesDTO>());
            }

            return PartialView("_PaginationPartial", apiResponse);
        }

        public IActionResult About() => View();
    }

}
