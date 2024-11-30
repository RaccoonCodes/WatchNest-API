using WatchNest.DTO;

namespace WatchNest.Models.Interfaces
{
    public interface IAdminService
    {
        Task<bool> DeleteUserAsync(string userId);
        Task<RestDTO<IEnumerable<object>>> GetAllUsersAsync(string baseurl, string rel, string action, int pageIndex = 0, int pageSize = 10);
        Task<RestDTO<IEnumerable<string>>> GetAllSeriesAsync(AdminRequestDTO<SeriesDTO> input, string baseUrl, string rel, string action);
    }
}
