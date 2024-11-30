using System.ComponentModel.DataAnnotations;

namespace WatchNest.DTO
{
    public class SeriesDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string UserID { get; set; } = string.Empty;
        [Required]
        public string TitleWatched { get; set; } = string.Empty;
        [Required]
        public int SeasonWatched { get; set; }
        [Required]
        public string ProviderWatched { get; set; } = string.Empty;
        [Required]
        public string Genre { get; set; } = string.Empty;
    }
}
