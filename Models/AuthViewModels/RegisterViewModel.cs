using System.ComponentModel.DataAnnotations;

namespace LinkedHUCENGv2.Models.AuthViewModels;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Name")]
    public string FirstName { get; set; }
    [Required]
    [Display(Name = "Last name")]
    public string LastName { get; set; }
    [Required]
    public int AccountType { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name ="Confirm Password")]
    [Compare("Password",ErrorMessage ="Password and confirmation password not match.")]
    public string ConfirmPassword { get; set; }

}