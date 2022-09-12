using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TodoBackend.Authentication;

namespace TodoBackend.Models
{
    public class TagModel
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public int PostId { get; set; }

        [JsonIgnore]
        public PostModel Post { get; set; }
    }
}
