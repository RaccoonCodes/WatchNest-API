using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WatchNest.Constants;
using WatchNest.DTO;
using WatchNest.Models;
using WatchNest.Models.Interfaces;

namespace WatchNest.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = $"{RoleNames.User},{RoleNames.Administrator}")]
    public class SeriesController : ControllerBase
    {
        private readonly ISeriesService _seriesService;

        public SeriesController(ISeriesService seriesService) =>
            _seriesService = seriesService;

        //CREATE
        [HttpPost(Name = "CreateSeries")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Creates a new Series",
            Description = "Creates a new series and stores it into the Database"
            )]
        public async Task<ActionResult<RestDTO<SeriesModel>>> Post([FromBody] SeriesDTO input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var results = await _seriesService.CreateSeriesAsync(input);

                results.Links.Add(new LinkDTO(
                       Url.Action(nameof(GetByIdInfo), "Series", new { id = results.Data.SeriesID },
                       Request.Scheme)!, "self", "POST"));

                return CreatedAtAction(nameof(GetByIdInfo), new { id = results.Data.SeriesID }, results);
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }

        }

        //READ 
        [HttpGet(Name = "GetSeries")]
        [ResponseCache(CacheProfileName = "Any-60")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Gets collection of series by users info",
            Description = "Retrieves a list of Series with paging, sorting, and filtering rules"
        )]
        public async Task<ActionResult<RestDTO<SeriesModel[]>>> Get([FromQuery] RequestDTO<SeriesDTO> input)
        {
            var results = await _seriesService.GetSeriesAsync(input, Url.Action
                    (null, "Series", null, Request.Scheme)!, "Self", "GET");

            if (!results.Data.Any())
            {
                return results.Message != null
                    ? Ok(results.Message)
                    : BadRequest("Invalid pagination parameters. Ensure 'pageIndex' >= 0 and 'pageSize' > 0.");
            }

            return Ok(results);
        }

        //UPDATE
        [HttpPut(Name = "UpdateSeries")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(
            Summary = "Updates a series",
            Description = "Updates existing series based on user's input"
        )]
        public async Task<ActionResult<SeriesModel?>> Put([FromBody] SeriesDTO model)
        {
            var result = await _seriesService.UpdateSeriesAsync(model, Url.Action(
                nameof(GetByIdInfo), "Series", new {id = model.Id},
                       Request.Scheme)!, "self", "GET");

            if (result?.Data == null)
            {
                return Conflict(result?.Message);
            }

            return Ok(result);
        }

        //DELETE
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Deletes a series in the list",
            Description = "Deletes a series from the database based on SeriesID"
        )]
        public async Task<ActionResult<SeriesModel?>> Delete(
            [SwaggerParameter("An id that represents SeriesID to be deleted")] int id)
        {
            var results = await _seriesService.DeleteSeriesAsync(id);

            if (results?.Data == null)
            {
                return NotFound(results?.Message);
            }
            
            return Ok(results);

        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Retrieves a singular series",
            Description = "Used to return information for a specifc series based on seriesID"
        )]
        public async Task<ActionResult<SeriesModel>?> GetByIdInfo(
            [SwaggerParameter("An id that represents SeriesID")] int id)
        {
            var result = await _seriesService.GetSeriesByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { Message = "Series not found." });
            }
            return Ok(result);
        }

    }
}
