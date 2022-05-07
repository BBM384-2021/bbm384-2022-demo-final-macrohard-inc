using System.ComponentModel.DataAnnotations;

namespace LinkedHUCENGv2.Models
{
    public class PDF
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public Post? Post { get; set; }
    }
}