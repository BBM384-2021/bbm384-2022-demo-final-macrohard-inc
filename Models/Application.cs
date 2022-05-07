using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    [Required]
    public List<Resume>? Resumes { get; set; }
    public List<Certificate>? Certificates { get; set; } = new List<Certificate>();
    [NotMapped]
    public IFormFile[]? CertificateFiles { get; set; }
    [NotMapped]
    public IFormFile[]? ResumeFile { get; set; }
}