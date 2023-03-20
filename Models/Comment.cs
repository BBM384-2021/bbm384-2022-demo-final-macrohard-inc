using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkedHUCENGv2.Models;

public class Comment
{
    [Key]
    public int CommentId { get; set; }
    [DataType(DataType.DateTime)]
    public DateTime DateCreated { get; set; }
    [Required]

    
    public string? CommentContent { get; set; }
    public string AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Account Account { get; set; }
    
    public Post Post { get; set; }
}