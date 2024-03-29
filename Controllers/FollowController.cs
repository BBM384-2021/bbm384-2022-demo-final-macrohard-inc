#nullable disable
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using static LinkedHUCENGv2.Utils.UserUtils;

namespace LinkedHUCENGv2.Controllers;
public class FollowController : Controller
{
    private readonly ApplicationDbContext _context;

    public FollowController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Follow
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.Follows.Include(f => f.Account1).Include(f => f.Account2);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: Follow/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var follow = await _context.Follows
            .Include(f => f.Account1)
            .Include(f => f.Account2)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (follow == null)
        {
            return NotFound();
        }

        return View(follow);
    }


    [HttpGet]
    [ActionName("IsFollowed")]
    public bool IsUserFollowed(string userId, string userIdToFollow)
    {
        var followingUsers = _context.Follows.Where(a => a.Account1Id == userId);
        foreach (var following in followingUsers)
            if (userIdToFollow == following.Account2Id)
                return true;
        return false;
    }

    public int GetFollowingCount(string userId)
    {
        var followingUsers = _context.Follows.Where(a => a.Account1Id == userId).ToList();
        return followingUsers.Count;
    }


    public int GetFollowerCount(string userId)
    {
        var followerUsers = _context.Follows.Where(a => a.Account2Id == userId).ToList();
        return followerUsers.Count;
    }

    [HttpGet]
    public async Task<IActionResult> GetFollowingList(string userId)
    {
        var followings = await _context.Follows.Where(a => a.Account1Id == userId).ToListAsync();
        var followingUsers = new List<Account>();
        var userLookedUp = await _context.Accounts.Where(a => a.Id == userId).FirstOrDefaultAsync();
        foreach (var follow in followings)
        {
            var user = await _context.Accounts.Where(a => a.Id == follow.Account2Id).FirstOrDefaultAsync();
            followingUsers.Add(user);
        }
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Login", "Account");
        var followControl = new FollowController(_context);
        var userProfileModel = GenerateUserProfileModel(currAcc, _context);
        ViewBag.UserName = GetFullName(userLookedUp);
        ViewBag.header = "Followings";
        ViewBag.color1 = "#CBCBCB";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "block";
        ViewBag.leftInside = "block";
        ViewBag.accountForViewBag = userProfileModel;
        return View("~/Views/Follow/ListAccounts.cshtml", followingUsers);
    }

    [HttpGet]
    public async Task<IActionResult> GetFollowersList(string userId)
    {
        var userLookedUp = await _context.Accounts.Where(a => a.Id == userId).FirstOrDefaultAsync();
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Login", "Account");
        var userProfileModel = GenerateUserProfileModel(currAcc, _context);

        var followers = await _context.Follows.Where(a => a.Account2Id == userId).ToListAsync();
        var followerUsers = new List<Account>();

        foreach (var follow in followers)
        {
            var user = await _context.Accounts.Where(a => a.Id == follow.Account1Id).FirstOrDefaultAsync();
            followerUsers.Add(user);
        }

        ViewBag.UserName = GetFullName(userLookedUp);
        ViewBag.header = "Followers";
        ViewBag.color1 = "#CBCBCB";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "block";
        ViewBag.leftInside = "block";
        ViewBag.accountForViewBag = userProfileModel;
        return View("~/Views/Follow/ListAccounts.cshtml", followerUsers);
    }


    [HttpGet]
    public async Task<IActionResult> FollowUser(string userId)
    {
        
        var userToFollow = await _context.Accounts.Where(m => m.Id == userId)

            .FirstOrDefaultAsync();
        var currUser = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        if (userToFollow is null)

            return Redirect("/Profile/ViewProfile?id=" + userId);
        if (currUser is null)
            return Redirect("/Profile/ViewProfile?id=" + userId);

        if (IsUserFollowed(currUser.Id, userId))
        {
            var unfollowFollow = await _context.Follows.FirstOrDefaultAsync(f => f.Account1.Id == currUser.Id && f.Account2.Id == userId);
            _context.Follows.Remove(unfollowFollow);
            await _context.SaveChangesAsync();
            return Redirect("/Profile/ViewProfile?id=" + userId);
        }

        var follow = new Follow
        {
            Account1 = currUser,
            Account1Id = currUser.Id,
            Account2 = userToFollow,
            Account2Id = userToFollow.Id,
            DateCreated = DateTime.Now
        };
        _context.Add(follow);
        var notificationController = new NotificationController(_context);
        notificationController.CreateFollowNotification(currUser, userToFollow);
        await _context.SaveChangesAsync();

        
        return Redirect("/Profile/ViewProfile?id="+ userId);

    }

    private bool FollowExists(int id)
    {
        return _context.Follows.Any(e => e.Id == id);
    }
}