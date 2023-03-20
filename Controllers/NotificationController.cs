using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using static LinkedHUCENGv2.Utils.UserUtils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkedHUCENGv2.Controllers;

public class NotificationController : Controller
{
    private ApplicationDbContext _context;
    public NotificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> LastNotifications()
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .Include(m => m.Notifications)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return Json(-1);
        var notifications = SortNotifications(currAcc.Notifications);
        return Json(notifications.Count > 3 ? notifications.GetRange(0, 3) : notifications);


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
        var notification = CreateNotification("like", GetFullName(accountWhoLiked) + " liked your post!");
        likedPost.Poster.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public void CreateCommentNotification(Account commenterAccount, Post commentedPost)
    {
        var notification = CreateNotification("comment", GetFullName(commenterAccount)
                                                         + " commented on your post!");
        commentedPost.Poster.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public void CreateFollowNotification(Account followerAccount, Account followingAccount)
    {
        var notification = CreateNotification("follow", GetFullName(followerAccount) + " is following you!");
        followingAccount.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public static List<Notification> SortNotifications(List<Notification> notifications)
    {
        var sort = notifications.OrderBy(n => DateTime.Now.Subtract(n.NotificationTime).TotalHours).ToList();
        return sort;

    }
    private static Notification CreateNotification(string notificationType, string notificationContent)
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