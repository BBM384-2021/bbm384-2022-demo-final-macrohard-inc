using LinkedHUCENGv2.Models;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models.AdminViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LinkedHUCENGv2.Controllers;

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
        var accounts = from a in _context.Accounts
            select a;
        if (!String.IsNullOrEmpty(searchString))
        {
            accounts = accounts.Where(a => a.FirstName.Contains(searchString)
                                           || a.LastName.Contains(searchString) || a.Email.Contains(searchString));
        }
        var viewModel = new AccountNotificationData();
        var notificationList = await _context.Notifications.Where(u => u.NotificationType == "register").ToListAsync();
        viewModel.Accounts = accounts.Any() ? accounts: new List<Account>();
        viewModel.Notifications = notificationList.Any() ? notificationList : new List<Notification>();
        return View(viewModel);
    }

    // GET: Admin/Details/5
    public async Task<IActionResult> Details(string? id)
    {
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

    // POST: Admin/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id,
        [Bind("Url,PhoneNumber,ProfilePhoto,AccountId,IsAdmin,FirstName,LastName,AccountType,Password,Email")]
        Account account)
    {
        if (id != account.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(account);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(account.AccountId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }
        
        return View(account);
    }

    // GET: Admin/Delete/5
    public async Task<IActionResult> Delete(string? id)
    {
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
        var account = await _context.Accounts.FindAsync(id);
        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    private bool AccountExists(int id)
    {
        return _context.Accounts.Any(e => e.AccountId == id);
    }
/*
    public void ExportToExcel()
    {
        ViewBag.userList = (List<AccountViewModel>) _context.Accounts.Select(x => new AccountViewModel
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                PhoneNumber = x.PhoneNumber,
                Url = x.Url

            }


        ).ToList();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        ExcelPackage pck = new ExcelPackage();
        ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");

        ws.Cells["A1"].Value = "First Name";
        ws.Cells["B1"].Value = "Last Name";

        ws.Cells["C1"].Value = "PhoneNumber";
        ws.Cells["D1"].Value = "LinkedHU_CENG Url";



        int rowStart = 2;
        foreach (var item in ViewBag.UserList)
        {


            ws.Cells[string.Format("A{0}", rowStart)].Value = item.FirstName;
            ws.Cells[string.Format("B{0}", rowStart)].Value = item.LastName;
            ws.Cells[string.Format("C{0}", rowStart)].Value = item.PhoneNumber;
            ws.Cells[string.Format("D{0}", rowStart)].Value = item.Url;
            rowStart++;
        }

        ws.Cells["A:AZ"].AutoFitColumns();
        Response.Clear();
        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        Response.Headers["content-disposition"] = "attachment: filename=" + "ExcelReport.xlsx";
        Response.Body.WriteAsync(pck.GetAsByteArray());

        

    }*/
}