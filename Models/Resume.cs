using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.CodeAnalysis.CodeFixes;

namespace LinkedHUCENGv2.Models;

public class Resume
{
    [Key]
    public int ResumeId { get; set; }
    [Required]
    public string? Name { get; set; } 
    [ForeignKey("ApplicationId")]
    public int ApplicationId { get; set; }
    [Required]
    public Application? Application { get; set; }
}