using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkedHUCENGv2.Models;

public class Follow
{
    [Key]
    public int Id { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime DateCreated { get; set; }
    
    public string Account1Id { get; set; }
    [ForeignKey("Account1Id")]
    public Account Account1 { get; set; }
    
    public string Account2Id { get; set; }
    [ForeignKey("Account2Id")]
    public Account Account2 { get; set; }
}