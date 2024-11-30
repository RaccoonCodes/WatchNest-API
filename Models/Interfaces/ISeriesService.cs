using WatchNest.DTO;

namespace WatchNest.Models.Interfaces
{
    public interface ISeriesService
    {
        Task<RestDTO<SeriesModel>> CreateSeriesAsync(SeriesDTO input);
        Task<RestDTO<SeriesModel[]>> GetSeriesAsync(RequestDTO<SeriesDTO> input, string baseurl, string rel, string action);
        Task<RestDTO<SeriesModel?>?> UpdateSeriesAsync(SeriesDTO model, string baseurl, string rel, string action);
        Task<RestDTO<SeriesModel?>> DeleteSeriesAsync(int id);
        Task<SeriesModel?> GetSeriesByIdAsync(int id);
    }
}
