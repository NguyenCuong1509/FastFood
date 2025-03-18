using System.ComponentModel.DataAnnotations;

namespace FastFoodOnline.Models
{
    public class Login
    {
        public string ReturnURL { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
