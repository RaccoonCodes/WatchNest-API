using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WatchNest.Models
{
    [Table("Series")]
    public class SeriesModel
    {
        public int SeriesID { get; set; }
        public string UserID { get; set; } = string.Empty;

        [Required(ErrorMessage ="Please enter Title")]
        public string TitleWatched { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter Season")]
        [Range(1,int.MaxValue, ErrorMessage = "Season must be a positive whole number")]
        public int SeasonWatched { get; set; }

        [Required(ErrorMessage ="Please enter Provider")]
        public string Provider {  get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter genre for this series")]
        public string Genre {  get; set; } = string.Empty;
        public ApiUsers? ApiUsers { get; set; } 
        public byte[]? RowVersion { get; set; } //Concurrency

    }
}
