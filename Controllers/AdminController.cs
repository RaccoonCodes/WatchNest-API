using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WatchNest.Constants;
using WatchNest.DTO;
using WatchNest.Models.Interfaces;
using WatchNestAPI.Models;

namespace WatchNest.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = RoleNames.Administrator)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
            => _adminService = adminService;


        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(
            Summary = "Deletes a user from the database",
            Description = "Deletes selected user and their associated series from the database. " +
            "Accessible only by administrators."
        )]
        public async Task<ActionResult> DeleteUser(
            [SwaggerParameter("The ID of the user to be deleted")] string userId)
        {
            var successResults = await _adminService.DeleteUserAsync(userId);

            if (!successResults)
            {
                return NotFound($"Error: Unable to delete user ID {userId} from the database");
            }

            return NoContent();
        }

        [HttpGet]
        //[ResponseCache(CacheProfileName = "Any-60")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Retrieves all users from the database",
            Description = "Returns a list of users, their IDs, and their roles"
        )]
        public async Task<ActionResult<RestDTO<IEnumerable<UserModel>>>> GetAllUsers(
            [SwaggerParameter("An int for starting page")][FromQuery] int pageIndex = 0,
             [SwaggerParameter("An int for number of entity per page")][FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _adminService.GetAllUsersAsync(Url.Action
                    (null, "Admin", null, Request.Scheme)!,"Self","GET", pageIndex, pageSize);

                if (!result.Data.Any())
                {
                    return string.IsNullOrEmpty(result.Message)
                        ? Ok(new RestDTO<IEnumerable<UserModel>>
                        {
                            Data = Enumerable.Empty<UserModel>(),
                            Message = "No users found!"
                        })
                        : BadRequest(result.Message);
                }

                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new RestDTO<string>
                    {
                        Message = $"An error occurred: {ex.Message}"
                    });
            }
        }


        [HttpGet]
       // [ResponseCache(CacheProfileName = "Any-60")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Retrieves all series from the database",
            Description = "Returns a list of series available in the database that are distinct with" +
            " pagination, sorting, and filter"
        )]
        public async Task<ActionResult<RestDTO<IEnumerable<SeriesDTO>>>> GetAllSeries([FromQuery] AdminRequestDTO<SeriesDTO> input)
        {
            try
            {

                var result = await _adminService.GetAllSeriesAsync(input, Url.Action
                   (null, "Admin", null, Request.Scheme)!,"Admin","GET");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new RestDTO<string>
                    {
                        Message = $"An error occurred: {ex.Message}"
                    });
            }
        }
    }
}
