using System.ComponentModel.DataAnnotations;

namespace LinkedHUCENGv2.Models;

public class Certificate
{
    [Key]
    public int CertificateId { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required]
    public Application? Application { get; set; }
}