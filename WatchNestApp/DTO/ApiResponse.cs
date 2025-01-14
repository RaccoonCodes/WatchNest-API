namespace WatchNestApp.DTO
{
    public class ApiResponse<T>
    {
        public List<T>? Data { get; set; }
        public int PageIndex { get; set; } = 0;
        public int RecordCount { get; set; } = 0;
        public int TotalPages { get; set; } = 0;
        public List<LinkDTO> Links { get; set; } = new List<LinkDTO>();


    }
}
