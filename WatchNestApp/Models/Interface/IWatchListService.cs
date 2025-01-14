using WatchNestApp.DTO;

namespace WatchNestApp.Models.Interface
{
    public interface IWatchListService
    {
        Task<ApiResponse<SeriesDTO>?> GetSeriesAsync(string userId, bool forceRefresh);
        Task<ApiResponse<SeriesDTO>?> FilterSeriesAsync(FilterDTO filterDTO);
        Task<UpdateModel?> GetSeriesByIdAsync(int infoID);
        Task<bool> UpdateSeriesAsync(UpdateSeriesDTO updateDTO);
        Task<bool> DeleteSeriesAsync(int infoID);
        Task<bool> AddSeriesAsync(NewSeriesDTO newSeriesDTO);
    }
}
