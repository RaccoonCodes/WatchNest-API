using Microsoft.Extensions.Caching.Distributed;
using System.Web;
using WatchNestApp.DTO;
using WatchNestApp.Extensions;
using WatchNestApp.Models.Interface;

namespace WatchNestApp.Models.Implementation
{
    public class WatchListService : IWatchListService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WatchListService> _logger;
        private readonly IDistributedCache _distributedCache;


        public WatchListService(IHttpClientFactory httpClientFactory, ILogger<WatchListService> logger,
            IDistributedCache distributedCache)
        => (_httpClientFactory, _logger, _distributedCache) = (httpClientFactory, logger, distributedCache);
        public async Task<ApiResponse<SeriesDTO>?> GetSeriesAsync(string userId, bool forceRefresh)
        {
            var cacheKey = userId.GenerateCacheKey();

            if (forceRefresh)
            {
                await _distributedCache.RemoveAsync(cacheKey);
            }

            if(_distributedCache.TryGetValue<ApiResponse<SeriesDTO>>(cacheKey, out ApiResponse<SeriesDTO>? cachedResponse))
            {
                return cachedResponse;
            }
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.GetAsync($"/Series/Get?PageSize=5&SortColumn=SeriesId&SortOrder=DESC&UserID={userId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent.Contains($"No series for the user with the UserID: {userId}"))
               {
                   return new ApiResponse<SeriesDTO>();
               }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<SeriesDTO>>();
            if (apiResponse != null)
                _distributedCache.Set(cacheKey, apiResponse, new TimeSpan(0, 3, 0)); // Cache for 3 minutes.

            return apiResponse;

        }
        public async Task<ApiResponse<SeriesDTO>?> FilterSeriesAsync(FilterDTO filterDTO)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var queryParams = HttpUtility.ParseQueryString(string.Empty);

            queryParams["UserID"] = filterDTO.UserID;
            if (!string.IsNullOrEmpty(filterDTO.SortColumn))
                queryParams["SortColumn"] = filterDTO.SortColumn;
            if (!string.IsNullOrEmpty(filterDTO.SortOrder))
                queryParams["SortOrder"] = filterDTO.SortOrder;
            if (!string.IsNullOrEmpty(filterDTO.FilterQuery))
                queryParams["FilterQuery"] = filterDTO.FilterQuery;

            var uriBuilder = new UriBuilder(client.BaseAddress!)
            {
                Path = "Series/Get",
                Query = queryParams.ToString()
            };

            var response = await client.GetAsync(uriBuilder.ToString());
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent.Contains($"No series for the user with the UserID: {filterDTO.UserID}"))
            {
                return new ApiResponse<SeriesDTO>();
            }

            return await response.Content.ReadFromJsonAsync<ApiResponse<SeriesDTO>>();
        }
        public async Task<UpdateModel?> GetSeriesByIdAsync(int infoID)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.GetAsync($"/Series/GetByIdInfo/{infoID}");
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<UpdateModel>() : null;
        }

        public async Task<bool> AddSeriesAsync(NewSeriesDTO newSeriesDTO)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.PostAsJsonAsync("/Series/Post", newSeriesDTO);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSeriesAsync(int infoID)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.DeleteAsync($"/Series/Delete/{infoID}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSeriesAsync(UpdateSeriesDTO updateDTO)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.PutAsJsonAsync("/Series/Put", updateDTO);
            return response.IsSuccessStatusCode;
        }
    }
}
