using System.Web;
using WatchNestApp.DTO;

namespace WatchNestApp.Extensions
{
    public static class FilterDTOExtensions
    {
        public static string ToQueryString(this FilterDTO filterDTO)
        {
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["UserID"] = filterDTO.UserID;
            if (!string.IsNullOrEmpty(filterDTO.SortColumn))
                queryParams["SortColumn"] = filterDTO.SortColumn;
            if (!string.IsNullOrEmpty(filterDTO.SortOrder))
                queryParams["SortOrder"] = filterDTO.SortOrder;
            if (!string.IsNullOrEmpty(filterDTO.FilterQuery))
                queryParams["FilterQuery"] = filterDTO.FilterQuery;

            return queryParams.ToString()!;
        }
    }
}
