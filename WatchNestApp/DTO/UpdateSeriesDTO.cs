namespace WatchNestApp.DTO
{
    public class UpdateSeriesDTO
    {
        public string TitleWatched { get;  private set; } 

        public int? SeasonWatched { get; private set; }

        public string Provider { get; private set; } 

        public string Genre { get; private set; }

        public int SeriesID { get; private set; }
        public string UserID { get; private set; } 

        public UpdateSeriesDTO(string titleWatched, int? seasonWatched, string provider, string genre, int seriesID, string userID)
        {
            TitleWatched = titleWatched;
            SeasonWatched = seasonWatched;
            Provider = provider;
            Genre = genre;
            SeriesID = seriesID;
            UserID = userID;
        }
    }
}
