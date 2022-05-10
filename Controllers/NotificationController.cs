using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using LinkedHUCENGv2.Utils;
using static LinkedHUCENGv2.Utils.NameUtils;
using Microsoft.AspNetCore.Mvc;

namespace LinkedHUCENGv2.Controllers;

public class NotificationController : Controller
{
    private ApplicationDbContext _context;
    public NotificationController(ApplicationDbContext context)
    {
        _context = context;
    }
    public void CreateRegisterNotification(Account account)
    {
        var notification = CreateNotification("register",
            GetFullName(account) + " has registered to the system.");
        var admin = _context.Accounts.Where(u => u.IsAdmin).ToList().FirstOrDefault();
        if (admin is null) return;
        admin.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public void CreateLikeNotification(Account accountWhoLiked, Post likedPost)
    {
        var notification = CreateNotification("like", GetFullName(accountWhoLiked) + " has liked your post!");
        likedPost.Poster?.Notifications.Add(notification);
        _context.SaveChanges();
    }

    private Notification CreateNotification(string notificationType, string notificationContent)
    {
        return new Notification
        {
            NotificationType = notificationType,
            IsRead = false,
            NotificationTime = DateTime.Now,
            NotificationContent = notificationContent
        };
    }

}