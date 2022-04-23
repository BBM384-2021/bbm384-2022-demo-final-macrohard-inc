#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    // GET: Follow/Create
    public IActionResult Create()
    {
        ViewData["Account1Id"] = new SelectList(_context.Accounts, "Id", "Id");
        ViewData["Account2Id"] = new SelectList(_context.Accounts, "Id", "Id");
        return View();
    }

    // POST: Follow/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,DateCreated,Account1Id,Account2Id")] Follow follow)
    {
        if (ModelState.IsValid)
        {
            _context.Add(follow);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["Account1Id"] = new SelectList(_context.Accounts, "Id", "Id", follow.Account1Id);
        ViewData["Account2Id"] = new SelectList(_context.Accounts, "Id", "Id", follow.Account2Id);
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
    public async Task<JsonResult> GetFollowingList(string userId)
    {
        var followingUsers = await _context.Follows.Where(a => a.Account1Id == userId).ToListAsync();
        return Json(followingUsers);
    }
    
    [HttpGet]
    public async Task<JsonResult> GetFollowersList(string userId)
    {
        var followerUsers = await _context.Follows.Where(a => a.Account2Id == userId).ToListAsync();
        return Json(followerUsers);
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
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UnfollowUser(string userId)
    {
        var userToUnfollow =  await _context.Accounts.Where(m => m.Id == userId)
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
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Delete(int? id)
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

    // POST: Follow/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var follow = await _context.Follows.FindAsync(id);
        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FollowExists(int id)
    {
        return _context.Follows.Any(e => e.Id == id);
    }
}
