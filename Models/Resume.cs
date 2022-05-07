using System.ComponentModel.DataAnnotations;

namespace LinkedHUCENGv2.Models;

public class Resume
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string? Name { get; set; }
    public Application? Application { get; set; }
}