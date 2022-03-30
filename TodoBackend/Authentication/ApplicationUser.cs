using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TodoBackend.Models;

namespace TodoBackend.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "First name is required")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string Lastname { get; set; }

        //[Required(ErrorMessage = "Country is required")]
        //public string Country { get; set; }

        public virtual ICollection<TodoModel> Todos { get; set; }
    }
}
