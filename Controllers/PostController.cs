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
    public async Task<IActionResult> Create([Bind("PostId,PostContent,PostType")] Post post)
    {
        if (ModelState.IsValid)
        {
            var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
                .FirstOrDefaultAsync();
            if (currAcc is null)
                return NotFound();
            post.Poster = currAcc;
            post.PostTime = DateTime.Now;
            currAcc.Posts ??= new List<Post>();
            currAcc.Posts.Add(post);
            _context.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(post);
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

    [HttpGet, ActionName("ListPosts")]
    public async Task<JsonResult> ListPostsOfUser(string mail)
    {
        var posts = await _context.Post.Where(p => p.Poster.Email == mail).ToListAsync();
        return Json(posts);
    }

    private bool PostExists(int id)
    {
        return _context.Post.Any(e => e.PostId == id);
    }
}
