using System.ComponentModel.DataAnnotations;

namespace TodoBackend.Authentication
{
    public class UpdateUserModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }
    }
}
