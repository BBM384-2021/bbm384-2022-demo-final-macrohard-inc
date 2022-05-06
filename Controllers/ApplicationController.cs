using System.Security.Policy;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkedHUCENGv2.Controllers;

public class ApplicationController : Controller
{
    private readonly ApplicationDbContext _context;

    public ApplicationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateApplication([Bind("ApplicationId, ApplicationText, ApplicationDate, PostId")] Application application, int postId, string applicationText)
    {
        var currAcc = await _context.Accounts.Where(a => a.Email == User.Identity.Name).FirstOrDefaultAsync();
        if (currAcc is null)
            return NotFound();
        var currPost = await _context.Post.Where(p => p.PostId == postId).FirstOrDefaultAsync();
        if (currPost is null)
            return NotFound();
        application.Applicant = currAcc;
        application.Post = currPost;
        application.ApplicationDate = DateTime.Now;
        application.ApplicationText = applicationText;
        return View();
    }
}