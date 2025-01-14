namespace WatchNestApp.Models
{
    public class DeleteModel
    {
        public string TitleWatched { get; set; } = string.Empty;
        public int? SeasonWatched { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int SeriesID { get; set; } = 0;
        public string UserID { get; set; } = string.Empty;
    }
}
