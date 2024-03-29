using LinkedHUCENGv2.Models;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models.AdminViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LinkedHUCENGv2.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin
    public async Task<IActionResult> Index(string searchString)
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .Include(m => m.Notifications)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Index", "Home");
        if (!currAcc.IsAdmin)
            return RedirectToAction("Feed", "Post");
        var accounts = _context.Accounts.Where(a => !a.IsAdmin);
        if (!String.IsNullOrEmpty(searchString))
        {
            accounts = accounts.Where(a => a.FirstName.ToLower().Contains(searchString.ToLower())
                                           || a.LastName.ToLower().Contains(searchString.ToLower())
                                           || a.Email.ToLower().Contains(searchString.ToLower()));
        }

        var viewModel = new AccountNotificationData();
        var notificationList = await _context.Notifications
            .Where(u => u.NotificationType == "register" || u.NotificationType == "request")
            .OrderBy(u => u.NotificationTime).ToListAsync();
        notificationList.Reverse();
        viewModel.Accounts = accounts.Any() ? accounts : new List<Account>();
        viewModel.Notifications = notificationList.Any() ? notificationList : new List<Notification>();
        return View(viewModel);
    }

    public async Task<IActionResult> DeleteNotification(int? id)
    {
        if (id == null) return NotFound();
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();
        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Admin");
    }

    // GET: Admin/Details/5
    public async Task<IActionResult> Details(string? id)
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .Include(m => m.Notifications)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Index", "Home");
        if (!currAcc.IsAdmin)
            return RedirectToAction("Feed", "Post");
        if (id == null)
        {
            return NotFound();
        }

        var account = await _context.Accounts
            .FirstOrDefaultAsync(m => m.Id == id);
        if (account == null)
        {
            return NotFound();
        }

        return View(account);
    }


    // GET: Admin/Edit/5
    public async Task<IActionResult> Edit(string? id)
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .Include(m => m.Notifications)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Index", "Home");
        if (!currAcc.IsAdmin)
            return RedirectToAction("Feed", "Post");
        if (id == null)
        {
            return NotFound();
        }

        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
        {
            return NotFound();
        }

        return View(account);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> Edit(string id,
        [Bind("Url,Phone,ProfilePhoto,AccountId,IsAdmin,FirstName,LastName,AccountType,Password,Email")]
        Account account)
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .Include(m => m.Notifications)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Index", "Home");
        if (!currAcc.IsAdmin)
            return RedirectToAction("Feed", "Post");

        var user = await _context.Accounts.FindAsync(id);
        if (id != user.Id)
        {
            return NotFound();
        }

        ModelState.Remove("AccountType");
        if (ModelState.IsValid)
        {
            user.FirstName = account.FirstName;
            user.LastName = account.LastName;
            user.Phone = account.Phone;
            user.Url = account.Url;
            user.Email = account.Email;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }


    // GET: Admin/Delete/5
    public async Task<IActionResult> Delete(string? id)
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .Include(m => m.Notifications)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Index", "Home");
        if (!currAcc.IsAdmin)
            return RedirectToAction("Feed", "Post");
        if (id == null)
        {
            return NotFound();
        }

        var account = await _context.Accounts
            .FirstOrDefaultAsync(m => m.Id == id);
        if (account == null)
        {
            return NotFound();
        }

        return View(account);
    }

    // POST: Admin/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .Include(m => m.Notifications)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Index", "Home");
        if (!currAcc.IsAdmin)
            return RedirectToAction("Feed", "Post");
        var account = await _context.Accounts.FindAsync(id);
        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    private bool AccountExists(int id)
    {
        return _context.Accounts.Any(e => e.AccountId == id);
    }

    public void ExportToExcel()
    {
        ViewBag.userList = _context.Accounts.Where(u => u.IsAdmin == false).Select(x => new AccountViewModel
        {
            FirstName = x.FirstName,
            LastName = x.LastName,
            Url = x.Url,
            Email = x.Email

        }


        ).ToList();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        ExcelPackage pck = new ExcelPackage();
        ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");

        ws.Cells["A1"].Value = "First Name";
        ws.Cells["B1"].Value = "Last Name";

        ws.Cells["C1"].Value = "LinkedHU_CENG Url";
        ws.Cells["D1"].Value = "Mail Address";



        int rowStart = 2;
        foreach (var item in ViewBag.UserList)
        {


            ws.Cells[string.Format("A{0}", rowStart)].Value = item.FirstName;
            ws.Cells[string.Format("B{0}", rowStart)].Value = item.LastName;
            ws.Cells[string.Format("C{0}", rowStart)].Value = item.Url;
            ws.Cells[string.Format("D{0}", rowStart)].Value = item.Email;
            rowStart++;
        }

        ws.Cells["A:AZ"].AutoFitColumns();
        Response.Clear();
        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        Response.Headers["content-disposition"] = "attachment: filename=" + "ExcelReport.xlsx";
        Response.Body.WriteAsync(pck.GetAsByteArray());



    }
}