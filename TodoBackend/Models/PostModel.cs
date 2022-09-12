using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TodoBackend.Authentication;

namespace TodoBackend.Models
{
    public class PostModel
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public string ThumbnailName { get; set; }

        [NotMapped]
        public string ThumbnailUrl { get; set; }

        [NotMapped]
        public IFormFile ThumbnailFile { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [Required(ErrorMessage = "MetaTitle is required")]
        public string MetaTitle { get; set; }

        [Required(ErrorMessage = "MetaDescription is required")]
        public string MetaDescription { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual ICollection<TagModel> Tags { get; set; }

        public string UserId { get; set; }
        
        [JsonIgnore]
        public ApplicationUser User { get; set; }
    }
}
