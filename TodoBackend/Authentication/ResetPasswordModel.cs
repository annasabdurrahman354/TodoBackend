using System.ComponentModel.DataAnnotations;

namespace TodoBackend.Authentication
{
    public class ResetPasswordModel
    {
        [MinLength(8)]
        [Required(ErrorMessage = "Password is required")]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
