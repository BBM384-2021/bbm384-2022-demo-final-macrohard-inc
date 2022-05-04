using System.Drawing;

namespace LinkedHUCENGv2.Models;

public class PostViewModel
{
    public int? PostId { get; set; }
    public string? PosterId { get; set; }
    public string? PostContent { get; set; }
    public string? PostType { get; set; }
    public DateTime? PostTime { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AccountType { get; set; }
    
    public string? Email { get; set; }
}