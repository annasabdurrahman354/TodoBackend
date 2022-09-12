using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoBackend.Authentication
{
    public class UserModel
    {
        [Required]
        public string Username { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }

        [NotMapped]
        public string ImageUrl { get; set; }
    }
}
