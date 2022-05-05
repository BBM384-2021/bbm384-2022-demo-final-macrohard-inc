#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;

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
        {
            if (userIdToFollow == following.Account2Id)
            {
                return true;
            }
        }
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
        foreach (var follow in followings)
        {
            var user = await _context.Accounts.Where(a => a.Id == follow.Account2Id).FirstOrDefaultAsync();
            followingUsers.Add(user);
        }
        return View("~/Views/Follow/ListAccounts.cshtml", followingUsers);
    }

    [HttpGet]
    public async Task<IActionResult> GetFollowersList(string userId)
    {
        var followers = await _context.Follows.Where(a => a.Account2Id == userId).ToListAsync();
        var followerUsers = new List<Account>();
        foreach (var follow in followers)
        {
            var user = await _context.Accounts.Where(a => a.Id == follow.Account1Id).FirstOrDefaultAsync();
            followerUsers.Add(user);
        }

        return View("~/Views/Follow/ListAccounts.cshtml", followerUsers);
    }

    [HttpPost]
    public async Task<IActionResult> FollowUser(string userId)
    {
        var userToFollow = await _context.Accounts.Where(m => m.Id == userId)
            .FirstOrDefaultAsync();
        var currUser = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        if (userToFollow is null)
            return NotFound();
        if (currUser is null)
            return NotFound();

        if (IsUserFollowed(currUser.Id, userId))
        {
            return NotFound();
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
        await _context.SaveChangesAsync();
        return RedirectToAction("ViewProfile", "Profile", new { id = userId });
    }

    [HttpPost]
    public async Task<IActionResult> UnfollowUser(string userId)
    {
        var userToUnfollow = await _context.Accounts.Where(m => m.Id == userId)
            .FirstOrDefaultAsync();
        var currUser = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();

        if (userToUnfollow is null)
            return NotFound();
        if (currUser is null)
            return NotFound();

        var follow = _context.Follows.Where(f => f.Account1.Id == currUser.Id && f.Account2.Id == userId).FirstOrDefaultAsync();
        if (follow.Result is null)
            return NotFound();
        _context.Remove(follow.Result);
        return RedirectToAction("ViewProfile", "Profile");
    }

    private bool FollowExists(int id)
    {
        return _context.Follows.Any(e => e.Id == id);
    }
}