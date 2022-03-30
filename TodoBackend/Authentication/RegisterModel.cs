using System.ComponentModel.DataAnnotations;

namespace TodoBackend.Authentication
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "First name is required")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string Lastname { get; set; }

        //[Required(ErrorMessage = "Country is required")]
        //public string Country { get; set; }

        //[Phone]
        //[Required(ErrorMessage = "Country is required")]
        //public string PhoneNumber { get; set; }

        [MinLength(8)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

    }
}
