using LinkedHUCENGv2.Data;
using Microsoft.AspNetCore.Mvc;
using LinkedHUCENGv2.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkedHUCENGv2.Controllers;

public class ProfileController : Controller
{
    private readonly ILogger<ProfileController> _logger;
    private ApplicationDbContext _context;

    public ProfileController(ILogger<ProfileController> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
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
        var followControl = new FollowController(_context);
        if (viewerAcc == account)
            return RedirectToAction("Homepage", "Home");
        if (account is null)
            return NotFound();
        var viewedUser = new UserProfileModel
        {
            Id = account.Id,
            FirstName = account.FirstName,
            LastName = account.LastName,
            ProfileBio = account.ProfileBio,
            Phone = account.Phone,
            Url = account.Url,
            ProfilePhoto = account.ProfilePhoto,
            StudentNumber = account.StudentNumber,
            FollowStatus = followControl.IsUserFollowed(viewerAcc.Id, account.Id) ? "Following" : "Follow",
            FollowersCount = followControl.GetFollowerCount(account.Id),
            FollowingCount = followControl.GetFollowingCount(account.Id)
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
            StudentNumber = currentAccount.StudentNumber,
            FollowStatus = followControl.IsUserFollowed(viewerAcc.Id, account.Id) ? "Following" : "Follow",
            FollowersCount = followControl.GetFollowerCount(account.Id),
            FollowingCount = followControl.GetFollowingCount(account.Id)
        };

        var list = new List<UserProfileModel>
        {
            currUserProfileModel,
            viewedUser
        };
        return View(list);
    }

}