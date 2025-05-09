﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TodoBackend.Authentication;

namespace TodoBackend.Models
{
    public class TodoModel
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public string ImageName { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Column(TypeName = "bit")]
        public bool Status { get; set; }

        public string UserId { get; set; }

        [JsonIgnore]
        public ApplicationUser User { get; set; }
    }
}
