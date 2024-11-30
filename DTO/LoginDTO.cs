using System.ComponentModel.DataAnnotations;

namespace WatchNest.DTO
{
    public class LoginDTO
    {
        [Required]
        public string? UserName { get; set; }
        
        [Required]
        public string? Password { get; set; }

    }
}
