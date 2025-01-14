namespace WatchNestApp.DTO
{
    public class FilterDTO
    {
        public string? SortColumn { get; private set; }
        public string? SortOrder { get; private set; }
        public string? FilterQuery { get; private set; }
        public string? UserID { get; private set; }

        public FilterDTO(string? sortColumn, string? sortOrder, string? filterQuery, string? userID)
        {
            SortColumn = sortColumn;
            SortOrder = sortOrder;
            FilterQuery = filterQuery;
            UserID = userID;
        }
    }
}
