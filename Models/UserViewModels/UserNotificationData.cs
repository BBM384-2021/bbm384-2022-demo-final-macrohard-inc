namespace LinkedHUCENGv2.Models.UserViewModels;

public class UserNotificationData
{
    public IEnumerable<Account> Accounts { get; set; }
    public IEnumerable<Notification> Notifications{ get; set; }
}