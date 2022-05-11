#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using static LinkedHUCENGv2.Utils.PostUtils;
using static LinkedHUCENGv2.Utils.UserUtils;
using Microsoft.AspNetCore.Authorization;

namespace LinkedHUCENGv2.Controllers;

[Authorize]
public class PostController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;
    public PostController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> CreatePost([Bind("PostId,PostContent,PostTime,PostType,ImageFiles,PDFFiles")] Post post, string postContent, string postType)

    {
        if (!ModelState.IsValid) return await Feed();
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        var filePath = Path.Combine(_hostEnvironment.WebRootPath, "img/usercontent");
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        if (post.ImageFiles != null)
        {
            foreach (var item in post.ImageFiles)
            {
                var newFileName = Guid.NewGuid() + "." + item.FileName.Split(".").Last();
                var fullFileName = Path.Combine(filePath, newFileName);
                await using (var fileStream = new FileStream(fullFileName, FileMode.Create))
                {
                    await item.CopyToAsync(fileStream);
                }
                post.Images.Add(new Image { Name = newFileName});
            }
        }

        var filePath2 = Path.Combine(_hostEnvironment.WebRootPath, "pdf");
        if (!Directory.Exists(filePath2))

        {
            Directory.CreateDirectory(filePath2);
        }
        if (post.PDFFiles != null )
        {
            foreach (var item in post.PDFFiles)
            {
               
                var fullFileName = Path.Combine(filePath2, item.FileName);
                await using (var fileStream = new FileStream(fullFileName, FileMode.Create))
                {
                    await item.CopyToAsync(fileStream);
                }
                post.PDFs.Add(new PDF { Name = item.FileName });
            }
        }

        post.Poster = currAcc;
        post.PostContent = postContent;
        post.PostType = postType;
        post.PostTime = DateTime.Now;
        _context.Add(post);
        await _context.SaveChangesAsync();
        return await Feed();
    }

    public async Task<IActionResult> DeleteOnFeed(int? id, string userId)
    {
        if (id == null)
        {
            return NotFound();
        }
        var post = await _context.Post
            .FirstOrDefaultAsync(m => m.PostId == id && m.Poster.Id == userId);
        if (post == null)
        {
            return NotFound();
        }
        _context.Post.Remove(post);
        await _context.SaveChangesAsync();
        return RedirectToAction("Feed");
    }

    public async Task<IActionResult> DeleteOnProfile(int? id, string userId)
    {
        if (id == null)
        {
            return NotFound();
        }
        var post = await _context.Post
            .FirstOrDefaultAsync(m => m.PostId == id && m.Poster.Id == userId);
        if (post == null)

        {
            return NotFound();
        }
        _context.Post.Remove(post);
        await _context.SaveChangesAsync();
        return RedirectToAction("Homepage", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Feed()
    {
        var allPosts = new List<PostViewModel>();
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name).FirstOrDefaultAsync();
        var currPosts = await GetPostsOfUser(currAcc, _context);
        
        allPosts.AddRange(CreatePostViews(currPosts, currAcc));
        // get posts from the followings
        var followings = await _context.Follows.Where(f => f.Account1.Id == currAcc.Id).ToListAsync();
        foreach (var follow in followings)
        {
            var user = await _context.Accounts.Where(a => a.Id == follow.Account2Id).FirstOrDefaultAsync();
            if (user != null)
            {
                var otherUserPosts = await GetPostsOfUser(user, _context);
                allPosts.AddRange(CreatePostViews(otherUserPosts, user));
            }
        }
        // get announcements
        var users = await _context.Accounts.Where(p => p.Email != User.Identity.Name).ToListAsync();
        foreach (var user in users)
        {
            var announcements = await _context.Post.Where(p => p.PostType == "Announcement")
                .Include(p=>p.Comments)
                .Include(p=>p.Likes)
                .Include(p=>p.Images)
                .Include(p=>p.PDFs)
                .AsSplitQuery()
                .ToListAsync();;
            allPosts.AddRange(CreatePostViews(announcements, user));
        }
        var sortedPosts = SortPosts(allPosts);
        var tuple = new Tuple<UserProfileModel, List<PostViewModel>>(await Profile(), sortedPosts);

        ViewBag.color1 = "#8000FF";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "#240046";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "block";
        ViewBag.leftInside = "block";
        ViewBag.accountForViewBag = await Profile();
        ViewBag.announcementBlock = "100";
        ViewBag.otherBlocks = "100";
        Console.WriteLine(currAcc.AccountType);
        if (currAcc.AccountType == "Student")
        {
            Console.WriteLine("student");
            ViewBag.announcementBlock = "0";
            ViewBag.otherBlocks = "0";
        }
        if (currAcc.AccountType == "StudentRep") 
        {
            Console.WriteLine("studentRep");
            ViewBag.otherBlocks = "0";
        }
        return View("~/Views/Home/Feed.cshtml", tuple);
    }

    [HttpGet]
    private bool PostExists(int id)
    {
        return _context.Post.Any(e => e.PostId == id);
    }

    private static List<PostViewModel> SortPosts(IEnumerable<PostViewModel> posts)
    {
        var sort = posts.OrderBy(p => p.PostTime).ToList();
        return sort;
    }

    private async Task<UserProfileModel> Profile()
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return null;
        var userProfileModel = GenerateUserProfileModel(currAcc, _context);
        return userProfileModel;
    }

    private static IEnumerable<PostViewModel> CreatePostViews(IEnumerable<Post> posts, Account acc)
    {
        var retList = new List<PostViewModel>{};
        retList.AddRange(from post in posts
        where post is not null
        select new PostViewModel()
        {
            PosterAccount = acc,
            PostContent = post.PostContent,
            PostTime = DateTime.Now.Subtract(post.PostTime).TotalHours,
            PostId = post.PostId,
            AccountType = acc.AccountType,
            FirstName = acc.FirstName,
            LastName = acc.LastName,
            PosterId = acc.Id,
            PostType = post.PostType,
            Email = acc.Email,
            Images = post.Images,
            PDFs = post.PDFs,
            LikeCount = post.Likes.Count,
            Comments = CreateCommentViews(post.Comments)
        });

        return retList;
    }
    
}