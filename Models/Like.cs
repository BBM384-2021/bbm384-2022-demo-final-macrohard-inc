using System.ComponentModel.DataAnnotations;

namespace LinkedHUCENGv2.Models;

public class Like
{
    [Key]
    public int LikeId { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime DateCreated { get; set; }

    public Account Account { get; set; }
    
    public Post Post { get; set; }
}