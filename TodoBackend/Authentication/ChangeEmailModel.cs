using System.ComponentModel.DataAnnotations;

namespace TodoBackend.Authentication
{
    public class ChangeEmailModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string NewEmail { get; set; }
    }
}
