using LinkedHU_CENG.Models;
using Microsoft.AspNetCore.Mvc;
namespace LinkedHU_CENG.Controllers;
using OfficeOpenXml;
using System.Linq;


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

    public IActionResult SearchUser(string searchKey)
    {
        var userList = _context.Accounts.Where(x => x.FirstName.Contains(searchKey) || searchKey == null).ToList(); // searchs for name that contains searchkey
        ViewBag.UserList = userList;
        return View("~/Views/Home/Admin.cshtml");
    }

    

    public void ExportToExcel() 
    {
        ViewBag.userList = (List<AccountViewModel>)_context.Accounts.Select(x => new AccountViewModel
        {
            FirstName = x.FirstName,
            LastName = x.LastName,
            PhoneNumber = x.PhoneNumber,
            Url= x.Url

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
        Response.Headers["content-disposition"]= "attachment: filename=" + "ExcelReport.xlsx";
        Response.Body.WriteAsync(pck.GetAsByteArray());


        


    }

}
    
