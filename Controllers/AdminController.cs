using LinkedHU_CENG.Models;
using Microsoft.AspNetCore.Mvc;
namespace LinkedHU_CENG.Controllers;

public class AdminController : Controller
{
    private readonly Context _context = new Context();
    public IActionResult ListUser()
    {
        var userList = _context.Accounts.Where(u => u.IsAdmin == false).ToList();
        ViewBag.UserList = userList;
        return View("~/Views/Home/Admin.cshtml");
    }
    
    public IActionResult DeleteUser(int accountId)
    {
        var user = _context.Accounts.Find(accountId);
        _context.Accounts.Remove(user);
        _context.SaveChanges();
        ViewBag.UserList = _context.Accounts.Where(u => u.IsAdmin == false).ToList();
        return View("~/Views/Home/Admin.cshtml");
    }

    public IActionResult ListNotifications()
    {
        var admin = _context.Accounts.Where(u => u.IsAdmin == true).ToList()[0];
        return new NotificationController().ListNotifications(admin);
    }
}