namespace LinkedHUCENGv2.Models.UserViewModels;

public class UserViewModel
{
    public string? Url { get; set; }
        
    public int? PhoneNumber { get; set; }
    public string? ProfilePhoto { get; set; }
        
    public string Id { get; set; }
       
    public bool IsAdmin { get; set; }
        
    public string? FirstName { get; set; }
       
    public string? LastName { get; set; }
      
    public string? AccountType { get; set; }
        
    // [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Please enter a valid email")]
    public string? Email { get; set; }
}