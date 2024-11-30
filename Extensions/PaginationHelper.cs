using System.Web;
using WatchNest.DTO;

namespace WatchNest.Extensions
{
    public static class PaginationHelper
    {
        public static List<LinkDTO> GeneratePaginationLinks(string baseUrl, string rel, string action, int pageIndex,
            int pageSize, int totalPages,Dictionary<string,string>? additionalParams = null)
        {
            var links = new List<LinkDTO>();

            string BuildUrl(int index)
            {
                var queryParams = new Dictionary<string, string>()
                {
                    { "pageIndex", index.ToString() },
                    { "pageSize", pageSize.ToString() }
                };
                if (additionalParams != null)
                {
                    foreach (var param in additionalParams)
                    {
                        queryParams[param.Key] = param.Value;
                    }
                }
                var queryString = string.Join("&", queryParams
                    .Select(kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));
                return $"{baseUrl}?{queryString}";
            }

            // Self link
            links.Add(new LinkDTO(BuildUrl(pageIndex), "self", action));

            // Next link (if not on the last page)
            if (pageIndex + 1 < totalPages)
            {
                links.Add(new LinkDTO(BuildUrl(pageIndex + 1), "next", action));
            }

            // Previous link (if not on the first page)
            if (pageIndex > 0)
            {
                links.Add(new LinkDTO(BuildUrl(pageIndex - 1), "previous", action));
            }


            

            return links;
        }
    }
}
