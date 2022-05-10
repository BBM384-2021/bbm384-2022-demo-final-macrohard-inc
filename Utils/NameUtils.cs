using LinkedHUCENGv2.Models;
    
namespace LinkedHUCENGv2.Utils;

public class NameUtils
{
    public static string GetFullName(Account account)
    {
        return account.FirstName + " " + account.LastName;
    }
}