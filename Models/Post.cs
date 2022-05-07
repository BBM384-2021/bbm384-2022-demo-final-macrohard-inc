using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkedHUCENGv2.Models;

public class Post
{
    [Key]
    public int PostId { get; set; }
    public Account? Poster { get; set; }
    [Required]
    public string? PostContent { get; set; }
    [NotMapped]
    public IFormFile[]? ImageFiles { get; set; }
    [Required]
    public DateTime PostTime { get; set; }
    [Required]
    public string? PostType { get; set; }
    public List<Image> Images { get; set; } = new List<Image>();
    public List<PDF> PDFs { get; set; } = new List<PDF>();
    [NotMapped]
    public IFormFile[]? PDFFiles { get; set; }


}