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
        var filePath = Path.Combine(_hostEnvironment.WebRootPath, "img");
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        if (post.ImageFiles != null)
        {
            foreach (var item in post.ImageFiles)
            {
                var fullFileName = Path.Combine(filePath, item.FileName);
                await using (var fileStream = new FileStream(fullFileName, FileMode.Create))
                {
                    await item.CopyToAsync(fileStream);
                }
                post.Images.Add(new Image { Name = item.FileName });
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
                Console.WriteLine("yesPDf");
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
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        var currPosts = await _context.Post.Include(p=>p.Images).Include(p => p.PDFs).Where(p => p.Poster.Email == User.Identity.Name).ToListAsync();
        allPosts.AddRange(CreatePostViews(currPosts, currAcc));
        // get posts from the followings
        var followings = _context.Follows.Where(f => f.Account1.Id == currAcc.Id);
        foreach (var follow in followings)
        {
            var user = await _context.Accounts.Where(a => a.Id == follow.Account2Id).FirstOrDefaultAsync();
            if (user != null)
            {
                allPosts.AddRange(CreatePostViews(user.Posts, user));
            }
        }
        // get announcements
        var users = await _context.Accounts.Where(p => p.Email != User.Identity.Name).ToListAsync();
        foreach (var user in users)
        {
            var announcements = await _context.Post.Include(p => p.Images).Include(p => p.PDFs).Where(p => p.PostType == "Announcement" && p.Poster.Id == user.Id).ToListAsync();
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
        sort.Reverse();
        return sort;
    }

    private async Task<UserProfileModel> Profile()
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
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
        return userProfileModel;
    }

    private List<PostViewModel> CreatePostViews(List<Post> posts, Account acc)
    {
        return posts.Select(post => new PostViewModel
        {
            PosterAccount = acc,
            PostContent = post.PostContent,
            PostTime = DateTime.Now.Subtract(post.PostTime).Minutes,
            PostId = post.PostId,
            AccountType = acc.AccountType,
            FirstName = acc.FirstName,
            LastName = acc.LastName,
            PosterId = acc.Id,
            PostType = post.PostType,
            Email = acc.Email,
            Images = post.Images,
            PDFs= post.PDFs
        })
            .ToList();
    }


    //bu taþýnacak bir fonksiyon

    [HttpGet]
    public async Task<IActionResult> Apply()
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
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "block";
        ViewBag.leftInside = "block";
        ViewBag.accountForViewBag = userProfileModel;

        return View("~/Views/Post/Apply.cshtml");
    }


}