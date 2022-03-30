using System.ComponentModel.DataAnnotations;

namespace TodoBackend.Authentication
{
    public class ResetPasswordModel
    {
        [MinLength(8)]
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
