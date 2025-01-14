
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WatchNestApp.Middleware
{
    public class JwtClaimsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtClaimsMiddleware> _logger;

        public JwtClaimsMiddleware(RequestDelegate next, ILogger<JwtClaimsMiddleware> logger)
            => (_next,_logger) = (next,logger);

        public async Task InvokeAsync(HttpContext context)
        {
            var jwt = context.Request.Cookies["AuthToken"];

            if (!string.IsNullOrEmpty(jwt))
            {
                try
                {
                    //Decode
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(jwt);

                    // ClaimsIdentity from the token claims
                    var claims = new ClaimsIdentity(token.Claims, "Bearer");

                    // Attach claims to the HttpContext.User
                    context.User = new ClaimsPrincipal(claims);
                }
                catch
                {
                    _logger.LogError("There was an error with jwt Token. Please look into it!");
                    context.User = new ClaimsPrincipal();
                }
            }
            await _next(context);
        }

    }
}
