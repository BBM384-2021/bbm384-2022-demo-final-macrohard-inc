using System.ComponentModel.DataAnnotations;
namespace LinkedHU_CENG.Models;

public class Account 
{
    public string? Url { get; set; }
    [Phone]
    public int? PhoneNumber { get; set; }
    public string? ProfilePhoto { get; set; }
    [Key]
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
    [Required(ErrorMessage = "Please choose a password!")]
    public string? Password { get; set; }
    [Required(ErrorMessage = "Please write your email!")]
    [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Please enter a valid email")]
    public string? Email { get; set; }
    public List<Notification> Notifications { get; set; } = new List<Notification>();
}