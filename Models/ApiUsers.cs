using Microsoft.AspNetCore.Identity;

namespace WatchNest.Models
{
    // One to many relationship
    public class ApiUsers : IdentityUser
    {
        public ICollection<SeriesModel> Series { get; set; } = new List<SeriesModel>();
    }
}
