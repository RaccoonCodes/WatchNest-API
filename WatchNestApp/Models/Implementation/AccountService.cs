using System.Net;
using System.Text.Json;
using WatchNestApp.DTO;
using WatchNestApp.Models.Interface;

namespace WatchNestApp.Models.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IHttpClientFactory httpClientFactory, ILogger<AccountService> logger)
        => (_httpClientFactory, _logger) = (httpClientFactory, logger);


        public async Task<(bool IsSuccess, string? ErrorMessage, string? JwtCookie)> LoginAsync(string username, string password)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var loginDto = new LoginDTO { Username = username, Password = password };

            try
            {
                var response = await client.PostAsJsonAsync("Account/Login", loginDto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = string.Empty;
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        errorMessage = "Invalid username or password";
                    }
                    else if (response.StatusCode != HttpStatusCode.BadRequest)
                    {
                        errorMessage = "An unexpected error occurred. Please try again.";
                    }
                       
                    return (false, errorMessage, null);
                }

                var jwtCookie = response.Headers.TryGetValues("Set-Cookie", out var cookies)
                    ? cookies.FirstOrDefault()
                    : null;

                return (true, null, jwtCookie);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API is down.");
                return (false, "Error: API is down. Please try again later!", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login.");
                return (false, "An unexpected error occurred. Please contact support.", null);
            }

        }

        public async Task<bool> LogoutAsync()
        {
            var client = _httpClientFactory.CreateClient("APIClient");

            var response = await client.DeleteAsync("Account/Logout");

            return response.IsSuccessStatusCode;
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> RegisterAsync(string username, string password, string email)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var registerDTO = new RegisterDTO { Username = username, Password = password, Email = email };

            try
            {
                var response = await client.PostAsJsonAsync("Account/Register", registerDTO);
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await ExtractErrorMessageAsync(response);
                    return (false, errorMessage);
                }

                return (true, null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API is down.");
                return (false, "Error: API is down. Please try again later!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration.");
                return (false, "An unexpected error occurred. Please contact support.");
            }
        }
        private async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
        {
            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorDetails = JsonSerializer.Deserialize<ErrorDetails>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return errorDetails?.Detail ?? "An unexpected error occurred.";
                }
                catch (JsonException)
                {
                    _logger.LogError("Error parsing response content: {Content}", content);
                }
            }
            return "An unexpected error occurred.";
        }
    }
}
