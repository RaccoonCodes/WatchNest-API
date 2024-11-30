using Microsoft.AspNetCore.Mvc;
using WatchNest.DTO;
using WatchNest.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

[Route("[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IUserServices _userServices;

    public AccountController(IUserServices userServices) => _userServices = userServices;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary ="Register new users", 
        Description ="Contains a DTO to hold user's data to be validated and store on Database")]
    public async Task<ActionResult> Register(RegisterDTO input)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _userServices.RegisterAsync(input);

                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status201Created);
                }
                else
                {
                    throw new Exception(string.Format("Error {0}", 
                        string.Join(" ", result.Errors.Select(e => e.Description))));
                }
            }
            else
            {
                var details = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };
                return new BadRequestObjectResult(details);
            }
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
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Performs a user login", Description = "Contains a DTO that will hold User's credentials")]
    public async Task<ActionResult> Login(LoginDTO input)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var jwtString = await _userServices.LoginAsync(input);

                if (jwtString == null)
                {
                    var problemDetails = new ProblemDetails
                    {
                        Title = "Invalid login attempt",
                        Detail = "Credentials provided are incorrect.",
                        Status = StatusCodes.Status401Unauthorized,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                    };

                    return StatusCode(StatusCodes.Status401Unauthorized,problemDetails);
                }
                Response.Cookies.Append("AuthToken", jwtString);
                return StatusCode(StatusCodes.Status200OK, jwtString);
            }
            else
            {
                var details = new ValidationProblemDetails(ModelState)
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Status = StatusCodes.Status400BadRequest
                };
                return new BadRequestObjectResult(details);
            }
           
        }
        catch (Exception ex)
        {
            var exceptionDetails = new ProblemDetails();
            exceptionDetails.Detail = ex.Message;
            exceptionDetails.Status = StatusCodes.Status401Unauthorized;
            exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            return StatusCode(StatusCodes.Status401Unauthorized, exceptionDetails);
        }
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Logs out the user by clearing authentication cookie", 
        Description = "If the user is not logged in, it returns a status indicating the user is already logged out.")]

    public IActionResult Logout()
    {
        if (Request.Cookies["AuthToken"] == null)
        {
            return BadRequest(new
            {
                Message = "You are not logged in!"
            });
        }

        Response.Cookies.Delete("AuthToken");

        return Ok(new
        {
            Message = "You Have been logged out successfully!"
        });


    }
}
