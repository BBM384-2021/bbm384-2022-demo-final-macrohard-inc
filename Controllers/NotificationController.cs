using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using static LinkedHUCENGv2.Utils.UserUtils;
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

    public void CreateCommentNotification(Account commenterAccount, Post commentedPost)
    {
        var notification = CreateNotification("comment", GetFullName(commenterAccount)
                                                         + " has commented your post:");
        commentedPost.Poster?.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public void CreateFollowNotification(Account followerAccount, Account followingAccount)
    {
        var notification = CreateNotification("follow", GetFullName(followerAccount) + " has started to follow you.");
        followingAccount.Notifications.Add(notification);
        _context.SaveChanges();
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