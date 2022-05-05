using System.ComponentModel.DataAnnotations;

namespace LinkedHUCENGv2.Models;

public class Post
{
    [Key]
    public int PostId { get; set; }
    public Account? Poster { get; set; }
    [Required]
    public string? PostContent { get; set; }
    [Required]
    public DateTime PostTime { get; set; }
    [Required]
    public string? PostType { get; set; }
    
    

}