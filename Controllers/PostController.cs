#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using Microsoft.AspNetCore.Authorization;

namespace LinkedHUCENGv2.Controllers;

[Authorize]
public class PostController : Controller
{
    private readonly ApplicationDbContext _context;

    public PostController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Post
    public async Task<IActionResult> Index()
    {
        return View(await _context.Post.ToListAsync());
    }

    // GET: Post/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Post
            .FirstOrDefaultAsync(m => m.PostId == id);
        if (post == null)
        {
            return NotFound();
        }

        return View(post);
    }

    // GET: Post/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Post/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(string postContent, string postType)
    {
        if (!ModelState.IsValid) return await Feed();
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return NotFound();
        var post = new Post
        {
            Poster = currAcc,
            PostContent = postContent,
            PostTime = DateTime.Now,
            PostType = postType
        };
        _context.Add(post);
        await _context.SaveChangesAsync();
        return await Feed();
    }

    // GET: Post/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();
        var post = await _context.Post.FindAsync(id);
        if (post is null)
            return NotFound();
        return View(post);
    }

    // POST: Post/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("PostId,PostContent,PostTime,PostType")] Post post)
    {
        if (id != post.PostId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid) return View(post);
        try
        {
            _context.Update(post);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostExists(post.PostId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Post/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Post
            .FirstOrDefaultAsync(m => m.PostId == id);
        if (post == null)
        {
            return NotFound();
        }

        return View(post);
    }

    // POST: Post/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var post = await _context.Post.FindAsync(id);
        if (post is null)
            return NotFound();

        _context.Post.Remove(post);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Feed()
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Login", "Account");
        var posts = new List<Post>();
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
        var currPosts = await _context.Post.Where(p => p.Poster.Email == User.Identity.Name).ToListAsync();
        posts.AddRange(currPosts);
        // get posts from the followings
        var followings = _context.Follows.Where(f => f.Account1.Id == currAcc.Id);
        foreach (var follow in followings)
        {
            var user = await _context.Accounts.Where(a => a.Id == follow.Account2Id).FirstOrDefaultAsync();
            if (user != null)
            {
                posts.AddRange(user.Posts);
            }
        }
        // get announcements
        var announcements = await _context.Post.Where(p => p.PostType == "Announcement" && p.Poster.Email != User.Identity.Name).ToListAsync();
        posts.AddRange(announcements);
        var sortedPosts = SortPosts(posts);
        var tuple = new Tuple<UserProfileModel, List<Post>>(userProfileModel, sortedPosts);
        return View("~/Views/Home/Feed.cshtml", tuple);
    }

    [HttpGet]
    private bool PostExists(int id)
    {
        return _context.Post.Any(e => e.PostId == id);
    }

    private List<Post> SortPosts(List<Post> posts)
    {
        var sort = posts.OrderBy(p => p.PostTime).ToList();
        sort.Reverse();
        return sort;

    }
}