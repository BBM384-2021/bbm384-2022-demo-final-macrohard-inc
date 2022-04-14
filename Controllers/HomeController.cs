using System.Diagnostics;
using LinkedHUCENGv2.Data;
using Microsoft.AspNetCore.Mvc;
using LinkedHUCENGv2.Models;
using LinkedHUCENGv2.Models.UserViewModels;
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

    [HttpGet]
    public async Task<JsonResult> ListUsers()
    {
        var accounts = await _context.Accounts.Where(a => a.IsAdmin == false).ToListAsync();
        return Json(accounts);
    }

    [HttpGet]
    public async Task<IActionResult> ViewProfile(string? mail)
    {
        var viewerAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        var account = await _context.Accounts.Where(a => a.Email == mail).FirstOrDefaultAsync();
        if (viewerAcc == account)
            return RedirectToAction("Homepage");
        var viewedUser = new UserProfileModel
        {
            Id = account.Id,
            FirstName = account.FirstName,
            LastName = account.LastName,
            ProfileBio = account.ProfileBio,
            Phone = account.Phone,
            Url = account.Url,
            ProfilePhoto = account.ProfilePhoto,
            StudentNumber = account.StudentNumber
        };

        var currentAccounts = await _context.Accounts.Where(m => m.Email == User.Identity.Name).ToListAsync();
        var currentAccount = currentAccounts[0];
        var currUserProfileModel = new UserProfileModel
        {
            Id = currentAccount.Id,
            FirstName = currentAccount.FirstName,
            LastName = currentAccount.LastName,
            ProfileBio = currentAccount.ProfileBio,
            Phone = currentAccount.Phone,
            Url = currentAccount.Url,
            ProfilePhoto = currentAccount.ProfilePhoto,
            StudentNumber = currentAccount.StudentNumber
        };

        var list = new List<UserProfileModel>();
        list.Add(currUserProfileModel);
        list.Add(viewedUser);
        return View(list);
    }


    public async Task<IActionResult> Homepage()
    {
        Account currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        UserProfileModel userProfileModel = new UserProfileModel
        {
            Id = currAcc.Id,
            FirstName = currAcc.FirstName,
            LastName = currAcc.LastName,
            ProfileBio = currAcc.ProfileBio,
            Phone = currAcc.Phone,
            Url = currAcc.Url,
            ProfilePhoto = currAcc.ProfilePhoto,
            //FollowersCount = currAcc.Followers.Count(),
            //FollowingCount = currAcc.Following.Count(),
            StudentNumber = currAcc.StudentNumber
        };
        return View(userProfileModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, [Bind("FirstName,LastName,Phone,Url, ProfileBio")] Account account)
    {
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
}