using LinkedHU_CENG.Models;
using Microsoft.AspNetCore.Mvc;
namespace LinkedHU_CENG.Controllers;

public class AdminController : Controller
{
    private readonly Context _context = new Context();
    
    public void ListNotifications()
    {
        var notificationList = _context.Notifications.Where(u => u.NotificationType == "register").ToList();
        ViewBag.Notifications = notificationList.Any() ? notificationList : new List<Notification>();
    }
    
    public void ListUser()
    {
        var userList = _context.Accounts.Where(u => u.IsAdmin == false).ToList();
        ViewBag.UserList = userList.Any() ? userList : new List<Account>();
    }

    // displays the admin panel with notifications and registered users
    public IActionResult DisplayAdminPanel()
    {
        ListNotifications();
        ListUser();
        return View("~/Views/Home/Admin.cshtml");
    }
    
   
    public IActionResult DeleteUser(int accountId)
    {
        var user = _context.Accounts.Find(accountId);
        _context.Accounts.Remove(user);
        _context.SaveChanges();
        return RedirectToAction("ListUser");
    }
}
     