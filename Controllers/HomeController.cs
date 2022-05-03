using System.Diagnostics;
using LinkedHUCENGv2.Data;
using Microsoft.AspNetCore.Mvc;
using LinkedHUCENGv2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LinkedHUCENGv2.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger,
                          ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (!User.Identity.IsAuthenticated) return View();
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        return currAcc.IsAdmin ? RedirectToAction("Index", "Admin") : RedirectToAction("Homepage");
    }



    public async Task<IActionResult> Homepage()
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
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
        ViewBag.color2 = "#8000FF";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "#240046";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "none";
        return View(userProfileModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, [Bind("FirstName,LastName,Phone,Url, ProfileBio")] Account account)
    {
        var user = await _context.Accounts.FindAsync(id);
        if (user is null)
            return NotFound();
        if (id != user.Id)
            return NotFound();

        ModelState.Remove("AccountType");
        if (ModelState.IsValid)
        {
            user.FirstName = account.FirstName;
            user.LastName = account.LastName;
            user.Phone = account.Phone;
            user.Url = account.Url;
            user.ProfileBio = account.ProfileBio;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public async Task<IActionResult> Feed()
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
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
        ViewBag.color1 = "#8000FF";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "#240046";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "block";
        return View(userProfileModel);
    }
}