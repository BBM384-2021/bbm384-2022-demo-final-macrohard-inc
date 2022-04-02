using LinkedHU_CENG.Models;
using Microsoft.AspNetCore.Mvc;
namespace LinkedHU_CENG.Controllers;

public class NotificationController : Controller
{
    private readonly Context _context = new Context();
    
    public IActionResult ListNotifications(Account account)
    {
        var notifications = _context.Accounts.Where(u => u.AccountId == account.AccountId).ToList()[0].Notifications;
        var unreadNotifications = new List<Notification>(); // do not show already read notifications
        int i;
        for (i = 0; i < notifications.Capacity; i++)
        {
            if (!notifications[i].IsRead)
            {
                unreadNotifications.Add(notifications[i]);
            }
        }
        ViewBag.Notifications = unreadNotifications;
        return View("~/Views/Home/Admin.cshtml");
    }

    public void CreateRegisterNotification(Account account)
    {
        var notification = new Notification();
        notification.NotificationType = "register";
        notification.IsRead = false;
        notification.NotificationTime = DateTime.Now;
        notification.NotificationContent = account.FirstName + " " + account.LastName + " has registered to the system.";
        var admin = _context.Accounts.Where(u => u.IsAdmin == true).ToList()[0];
        admin.Notifications.Add(notification);
    }
    
}