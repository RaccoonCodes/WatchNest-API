namespace WatchNestApp.DTO
{
    public class SeriesDTO
    {
        public int SeriesID { get; set; } = 0;
        public string TitleWatched {  get; set; } = string.Empty;
        public int SeasonWatched {  get; set; } = 0;
        public string Provider { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
    }
}
