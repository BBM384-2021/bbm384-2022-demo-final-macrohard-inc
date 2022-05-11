namespace LinkedHUCENGv2.Models;

public class NotificationViewModel 
{
    public int? NotificationId { get; set; }
    public double? NotificationTime { get; set; }
    public string? NotificationContent { get; set; }
    public Account? Notifier{ get; set; }
}