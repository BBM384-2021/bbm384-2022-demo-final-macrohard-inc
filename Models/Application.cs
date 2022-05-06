using System.ComponentModel.DataAnnotations;

namespace LinkedHUCENGv2.Models;

public class Application
{
    [Key]
    public int ApplicationId { get; set; }
    public string? ApplicationText { get; set; }
    [Required]
    public DateTime? ApplicationDate { get; set; }
    [Required]
    public Account? Applicant { get; set; }
    [Required]
    public int PostId { get; set; }
    public Post? Post { get; set; }
}