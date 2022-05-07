#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;

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
    
    public IActionResult Create()
    {
        var currAcc = _context.Accounts
            .FirstOrDefault(m => m.Email == User.Identity.Name);
        if (currAcc is null)
            return RedirectToAction("Login", "Account");
        var followControl = new FollowController(_context);
        var userProfileModel = new UserProfileModel
        {
            Id = currAcc.Id,
            FirstName = currAcc.FirstName,
            LastName = currAcc.LastName,
            ProfileBio = currAcc.ProfileBio,
            Phone = currAcc.Phone,
            Url = currAcc.Url,
            ProfilePhoto = currAcc.ProfilePhoto,
            FollowersCount = followControl.GetFollowerCount(currAcc.Id),
            FollowingCount = followControl.GetFollowingCount(currAcc.Id),
            StudentNumber = currAcc.StudentNumber
        };
        ViewBag.color1 = "#CBCBCB";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "block";
        ViewBag.leftInside = "block";
        ViewBag.accountForViewBag = userProfileModel;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ApplicationId,ApplicationText,ApplicationDate,PostId, ResumeFile, CertificateFiles")] Application application)
    {
        var postId = Convert.ToInt32(TempData["postId"]);
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Login", "Account");
        var filePath = Path.Combine(_hostEnvironment.WebRootPath, "pdf");
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);
        if (application.ResumeFile is not null)
        {
            foreach (var item in application.ResumeFile)
            {
                application.Resumes = new List<Resume>();
                var fullFileName = Path.Combine(filePath, item.FileName);
                await using (var fileStream = new FileStream(fullFileName, FileMode.Create))
                {
                    await item.CopyToAsync(fileStream);
                }
                application.Resumes.Add(new Resume { Name = item.FileName });
            }
            
        }
        application.Post = await _context.Post.Where(p => p.PostId == postId).FirstOrDefaultAsync();
        application.PostId = postId;
        application.ApplicationDate = DateTime.Now;
        application.Applicant = currAcc;
        _context.Add(application);
        await _context.SaveChangesAsync();
        var followControl = new FollowController(_context);
        var userProfileModel = new UserProfileModel
        {
            Id = currAcc.Id,
            FirstName = currAcc.FirstName,
            LastName = currAcc.LastName,
            ProfileBio = currAcc.ProfileBio,
            Phone = currAcc.Phone,
            Url = currAcc.Url,
            ProfilePhoto = currAcc.ProfilePhoto,
            FollowersCount = followControl.GetFollowerCount(currAcc.Id),
            FollowingCount = followControl.GetFollowingCount(currAcc.Id),
            StudentNumber = currAcc.StudentNumber
        };
        ViewBag.color1 = "#CBCBCB";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "block";
        ViewBag.leftInside = "block";
        ViewBag.accountForViewBag = userProfileModel;
        return View(application);
    }
}