#nullable disable
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static LinkedHUCENGv2.Utils.UserUtils;

namespace LinkedHUCENGv2.Controllers;

public class ApplicationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ApplicationController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    public IActionResult Create(int postId)
    {

        var currAcc = _context.Accounts
            .FirstOrDefault(m => m.Email == User.Identity.Name);
        if (currAcc is null)
            return RedirectToAction("Login", "Account");
        var userProfileModel = GenerateUserProfileModel(currAcc, _context);
        ViewBag.color1 = "#CBCBCB";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "block";
        ViewBag.leftInside = "block";
        ViewBag.accountForViewBag = userProfileModel;
        ViewBag.postId = postId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string applicationText, IFormFile[] resumeFiles,int postId)
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Login", "Account");
        var filePath = Path.Combine(_hostEnvironment.WebRootPath, "pdf");
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        var userProfileModel = GenerateUserProfileModel(currAcc, _context);
        ViewBag.color1 = "#CBCBCB";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "block";
        ViewBag.leftInside = "block";
        ViewBag.err = "";
        ViewBag.accountForViewBag = userProfileModel;
        ViewBag.postId = postId;
        var application = new Application
        {
            ApplicationText = applicationText,
            Applicant = currAcc,
            Post = await _context.Post.Where(p => p.PostId == postId).Include(p => p.Poster).FirstOrDefaultAsync(),
            ApplicationDate = DateTime.Now,
            ResumeFiles = resumeFiles
        };
        if (resumeFiles.Length == 0)
        {
            ViewBag.err = "resume";
            return View(application);
        }

        foreach (var item in application.ResumeFiles)
        {
            var fullFileName = Path.Combine(filePath, item.FileName);
            await using (var fileStream = new FileStream(fullFileName, FileMode.Create))
            {
                await item.CopyToAsync(fileStream);
            }
            application.Resumes.Add(new Resume { Name = item.FileName, Application = application });
        }

        SendApplicationMail(application, application.Post.Poster);
        _context.Add(application);
        await _context.SaveChangesAsync();
        return View(application);
    }

    private void SendApplicationMail(Application application, Account poster)
    {
        if (application.Applicant is null)
            return;
        var toMail = poster.Email;
        const string fromMail = "linkedhuceng.applications@gmail.com";

        var message = new MailMessage(fromMail, toMail);
        message.BodyEncoding = Encoding.UTF8;
        message.Subject = GetFullName(application.Applicant) + " has applied to your post!";
        message.Body = message.Subject + "\nHere is the application details below.\n" + application.ApplicationText
                       + "\n" + application.Applicant.Url;
        foreach (var resume in application.ResumeFiles)
        {
            var file = "wwwroot/pdf/" + resume.FileName;
            var data = new Attachment(file, MediaTypeNames.Application.Octet);
            var disposition = data.ContentDisposition;
            disposition.CreationDate = System.IO.File.GetCreationTime(file);
            disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
            disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
            message.Attachments.Add(data);
        }
        var client = new SmtpClient("smtp.gmail.com", 587); //Gmail smtp    
        var myCred = new NetworkCredential(
            fromMail,"linkedhuceng");
        client.EnableSsl = true;  
        client.UseDefaultCredentials = false;  
        client.Credentials = myCred;  
        try   
        {  
            client.Send(message);  
        }   
  
        catch (Exception ex)   
        {  
            throw ex;  
        }  

    }
}