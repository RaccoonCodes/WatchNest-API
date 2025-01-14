using WatchNestApp.DTO;

namespace WatchNestApp.Models.Interface
{
    public interface IAdminService
    {
        Task<ApiResponse<UserModel>> GetAllUsersAsync();
        Task<ApiResponse<SeriesDTO>> GetAllSeriesAsync();
        Task<ApiResponse<SeriesDTO>> GetFilteredSeriesAsync(FilterDTO filterDTO);
        Task<ApiResponse<UserModel>> GetCachedUserListAsync(string cacheKey);
        Task RefreshCacheAsync(string cacheKey);
        Task<bool> DeleteUserAsync(string userID);

    }
}
