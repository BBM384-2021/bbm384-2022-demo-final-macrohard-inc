using System.ComponentModel.DataAnnotations.Schema;

namespace LinkedHUCENGv2.Models;

public class CommentViewModel
{
    public int? CommentId { get; set; }
    public string? CommentContent { get; set; }
    public double? CommentTime { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AccountType { get; set; }
    public string? Email { get; set; }
    public Account? Account { get; set; }
}