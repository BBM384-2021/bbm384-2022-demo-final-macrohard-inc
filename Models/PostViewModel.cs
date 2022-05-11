using System.ComponentModel.DataAnnotations.Schema;

using System.Drawing;

namespace LinkedHUCENGv2.Models;

public class PostViewModel
{
    public int? PostId { get; set; }
    public string? PosterId { get; set; }
    public string? PostContent { get; set; }
    public string? PostType { get; set; }
    public double? PostTime { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AccountType { get; set; }
    public Account? PosterAccount { get; set; }
    public string? Email { get; set; }
    public int? LikeCount { get; set; }
    [NotMapped]
    public IFormFile[]? ImageFiles { get; set; }
    public List<Image> Images { get; set; } = new List<Image>();
    public List<PDF> PDFs { get; set; } = new List<PDF>();
    [NotMapped]
    public IFormFile[]? PDFFiles { get; set; }
    public List<CommentViewModel>? Comments { get; set; } = new List<CommentViewModel>();
}