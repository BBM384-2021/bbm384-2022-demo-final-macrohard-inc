namespace LinkedHU_CENG.Models.AdminViewModels;

public class AccountNotificationData
{
    public IEnumerable<Account> Accounts { get; set; }
    public IEnumerable<Notification> Notifications{ get; set; }

}