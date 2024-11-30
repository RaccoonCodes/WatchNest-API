namespace WatchNest.DTO
{
    //Class containing data and links sent to the client
    //This serves more of outgoing and responding request
    public class RestDTO<T>
    {
        public T Data { get; set; } = default!;
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public int? RecordCount { get; set; }
        public int? TotalPages { get; set; }
        public string? Message { get; set; }
        public List<LinkDTO> Links { get; set; } = new List<LinkDTO>();
    }
}
