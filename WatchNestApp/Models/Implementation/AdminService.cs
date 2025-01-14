using Microsoft.Extensions.Caching.Distributed;
using System.Web;
using WatchNestApp.DTO;
using WatchNestApp.Extensions;
using WatchNestApp.Models.Interface;

namespace WatchNestApp.Models.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AdminService> _logger;
        private readonly IDistributedCache _distributedCache;

        public AdminService(IHttpClientFactory httpClientFactory, ILogger<AdminService> logger, IDistributedCache distributedCache)
        => (_httpClientFactory, _logger, _distributedCache) = (httpClientFactory, logger, distributedCache);
        public async Task<ApiResponse<UserModel>> GetAllUsersAsync()
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.GetAsync("/Admin/GetAllUsers");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve users. Status Code: {StatusCode}", response.StatusCode);
                return null!;
            }

            return await response.Content.ReadFromJsonAsync<ApiResponse<UserModel>>() ?? new ApiResponse<UserModel>();
        }
        public async Task<ApiResponse<SeriesDTO>> GetAllSeriesAsync()
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.GetAsync("/Admin/GetAllSeries");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve series. Status Code: {StatusCode}", response.StatusCode);
                return null!;
            }

            return await response.Content.ReadFromJsonAsync<ApiResponse<SeriesDTO>>() ?? new ApiResponse<SeriesDTO>();


        }
        public async Task<ApiResponse<SeriesDTO>> GetFilteredSeriesAsync(FilterDTO filterDTO)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var queryParams = filterDTO.ToQueryString();

            var uriBuilder = new UriBuilder(client.BaseAddress!)
            {
                Path = "Series/Get",
                Query = queryParams
            };

            var response = await client.GetAsync(uriBuilder.ToString());

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve filtered series. Status Code: {StatusCode}", response.StatusCode);
                throw new Exception("Failed to fetch series.");
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            if (responseContent.Contains($"No series for the user with the UserID: {filterDTO.UserID}"))
            {
                return new ApiResponse<SeriesDTO>();
            }

            return await response.Content.ReadFromJsonAsync<ApiResponse<SeriesDTO>>() ?? new ApiResponse<SeriesDTO>();
        }
        public async Task<ApiResponse<UserModel>> GetCachedUserListAsync(string cacheKey)
        {
            if (_distributedCache.TryGetValue<ApiResponse<UserModel>>(cacheKey, out var cachedResponse))
            {
                return cachedResponse!;
            }

            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.GetAsync("/Admin/GetAllUsers");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve user list. Status Code: {StatusCode}", response.StatusCode);
                throw new Exception("Failed to fetch user list.");
            }

            var userList = await response.Content.ReadFromJsonAsync<ApiResponse<UserModel>>();



            if (userList != null)
            {
                _distributedCache.Set(cacheKey, userList, new TimeSpan(1, 30, 0)); // Cache for 1hr 30 mins
            }
            return userList ?? new ApiResponse<UserModel>();
        }

        public async Task RefreshCacheAsync(string cacheKey)
        {
            await _distributedCache.RemoveAsync(cacheKey);
        }

        public async Task<bool> DeleteUserAsync(string userID)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.DeleteAsync($"/Admin/DeleteUser/{userID}");

            return response.IsSuccessStatusCode;
        }

    }
}
