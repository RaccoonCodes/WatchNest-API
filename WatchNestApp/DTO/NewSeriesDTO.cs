namespace WatchNestApp.DTO
{
    public class NewSeriesDTO
    {
        public string UserID { get; private set; }
        public string TitleWatched { get; private set; }
        public int? SeasonWatched { get; private set; } 
        public string Provider { get; private set; }
        public string Genre {  get; private set; }

        public NewSeriesDTO(string userID, string titleWatched, int? seasonWatched, string providerWatched, string genre)
        {
            UserID = userID;
            TitleWatched = titleWatched;
            SeasonWatched = seasonWatched;
            Provider = providerWatched;
            Genre = genre;
        }
    }
}
