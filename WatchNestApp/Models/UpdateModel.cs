using System.ComponentModel.DataAnnotations;

namespace WatchNestApp.Models
{
    public class UpdateModel
    {
        [Required(ErrorMessage = "Title is required.")]
        public string TitleWatched { get; set; } = string.Empty;

        [Required(ErrorMessage = "Season is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Season must be a positive whole number")]
        public int? SeasonWatched { get; set; }

        [Required(ErrorMessage = "Provider is required.")]
        public string Provider { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required.")]
        public string Genre { get; set; } = string.Empty;

        public int SeriesID { get; set; } = 0;
        public string UserID { get; set; } = string.Empty;

    }
}
