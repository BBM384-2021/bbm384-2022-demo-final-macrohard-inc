using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace LinkedHUCENGv2.Models;

public class Account : IdentityUser
{
    public string? Url { get; set; }
    public string? Phone { get; set; }
    public string? ProfilePhoto { get; set; }
    [NotMapped]
    public IFormFile? ProfilePhotoFile { get; set; }
    public int AccountId { get; set; }
    [Required]
    public bool IsAdmin { get; set; }
    [Required(ErrorMessage = "Please write your first name!")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "First name should contain at least 3 letters.")]
    public string? FirstName { get; set; }
    [Required(ErrorMessage = "Please write your last name!")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name should contain at least 2 letters.")]
    public string? LastName { get; set; }
    [Required(ErrorMessage = "You should choose the type!")]
    public string? AccountType { get; set; }

    [NotMapped]
    public List<Follow> Following { get; set; } = new List<Follow>();
    [NotMapped]
    public List<Follow> Followers { get; set; } = new List<Follow>();
    public List<Notification> Notifications { get; set; } = new List<Notification>();
    [NotMapped]
    public IFormFile? ProfilePhotoFile { get; set; }
    [Required]
    public DateTime RegistrationDate { get; set; }
    public string? ProfileBio { get; set; }
    public string? StudentNumber { get; set; }
    public List<Post> Posts { get; set; } = new List<Post>();
}