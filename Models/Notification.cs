using System.ComponentModel.DataAnnotations;

namespace LinkedHUCENGv2.Models;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }
    [Required]
    public DateTime NotificationTime { get; set; }
    [Required]
    public string? NotificationContent { get; set; }
    [Required] // "connection" notification for users and "register" notification for admin
    public string? NotificationType { get; set; }
    [Required]
    public bool IsRead { get; set; }
}