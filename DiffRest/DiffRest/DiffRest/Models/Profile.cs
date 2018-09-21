using System.ComponentModel.DataAnnotations;

namespace DiffRest.Models
{
    public class Profile
    {
        [Required(ErrorMessage = "UserName is required")]
        public string Url { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}