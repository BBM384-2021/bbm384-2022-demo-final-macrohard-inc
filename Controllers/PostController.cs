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
        var likeController = new LikeController(_context);
        var followController = new FollowController(_context);
        var allPosts = new List<PostViewModel>();
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name).FirstOrDefaultAsync();
        var currPosts = await _context.Post.Where(p => p.Poster.Id == currAcc.Id)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Include(p => p.Images)
            .Include(p => p.PDFs)
            .AsSplitQuery()
            .ToListAsync();
        allPosts.AddRange(CreatePostViews(currPosts, currAcc));
        // get posts from the followings
        var followings = await _context.Follows.Where(f => f.Account1.Id == currAcc.Id).ToListAsync();
        foreach (var follow in followings)
        {
            var user = await _context.Accounts.Where(a => a.Id == follow.Account2Id).Include(a => a.Comments).FirstOrDefaultAsync();
            if (user == null) continue;
            var otherUserPosts = await GetPostsOfUser(user, _context);
            allPosts.AddRange(CreatePostViews(otherUserPosts, user));
        }
        // get announcements
        var users = await _context.Accounts.Where(p => p.Email != User.Identity.Name).ToListAsync();
        var announcements = await _context.Post.Where(p => p.PostType == "Announcement")
                .Include(p=>p.Comments)
                .Include(p=>p.Likes)
                .Include(p=>p.Images)
                .Include(p=>p.PDFs)
                .AsSplitQuery()
                .ToListAsync();;
        allPosts.AddRange(from announcement in announcements 
            let accViewModel = GeneratePostViewModel(announcement)
            where !(announcement.Poster == currAcc || followController.IsUserFollowed(currAcc?.Id, announcement.Poster.Id)) 
            select accViewModel);

        foreach (var post in allPosts)
        {
            post.IsLiked = await likeController.IsPostLikedAsync(currAcc.Id, post.PostId);
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
    
    public async Task<IActionResult> EditPost(int? id)
    {
        var currAcc = _context.Accounts
            .FirstOrDefault(m => m.Email == User.Identity.Name);
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
        ViewBag.announcementBlock = "100";
        ViewBag.otherBlocks = "100";
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
        ViewBag.accountForViewBag = userProfileModel;
        if (id is null)
            return NotFound();
        //var post = await _context.Post.FindAsync(id).Include(p => p.Images).Include(p => p.PDFs);
        var post = await _context.Post.Include(p => p.Images).Include(p => p.PDFs).Where(p => p.PostId == id).FirstOrDefaultAsync();
        if (post is null)
            return NotFound();
        return View(post);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, [Bind("PostId,PostContent,PostTime,PostType")] Post post)
    {
        var currAcc = _context.Accounts
            .FirstOrDefault(m => m.Email == User.Identity.Name);
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
        ViewBag.announcementBlock = "100";
        ViewBag.otherBlocks = "100";
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
        ViewBag.accountForViewBag = userProfileModel;
        if (!ModelState.IsValid) return View(post);

        var postAcc = await _context.Post.Include(p => p.Images).Include(p => p.PDFs).Where(p => p.PostId == id).FirstOrDefaultAsync();
        if (postAcc is null)
            return NotFound();
        postAcc.PostContent = post.PostContent;
        postAcc.PostType = post.PostType;
        _context.Update(postAcc);
        await _context.SaveChangesAsync();
        return View(postAcc);
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

    private IEnumerable<PostViewModel> CreatePostViews(IEnumerable<Post> posts, Account acc)
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
            Comments = SortComments(CreateCommentViews(post.Comments)),
            IsLiked = _context.Likes.FirstOrDefault(l => l.Account.Id == acc.Id && l.Post.PostId == post.PostId) != null
        }); ;

        return retList;
    }
    public PostViewModel GeneratePostViewModel(Post post)
    {
        var account = post.Poster;
        var like = _context.Likes.FirstOrDefault(l => l.Account.Id == account.Id && l.Post.PostId == post.PostId);
        var postViewModel = new PostViewModel
        {
            PosterAccount = account,
            PostContent = post.PostContent,
            PostTime = DateTime.Now.Subtract(post.PostTime).TotalHours,
            PostId = post.PostId,
            AccountType = account.AccountType,
            FirstName = account.FirstName,
            LastName = account.LastName,
            PosterId = account.Id,
            PostType = post.PostType,
            Email = account.Email,
            Images = post.Images,
            PDFs = post.PDFs,
            Comments = SortComments(CreateCommentViews(post.Comments)),
            LikeCount = post.Likes.Count,
            IsLiked = like != null
        };
        return postViewModel;
    }
    public List<CommentViewModel> CreateCommentViews(List<Comment> comments)
    {
        var retList = new List<CommentViewModel>();
        if (comments.Count == 0)
        {
            return retList;
        }

        foreach (var comment in comments)
        {

            var acc = _context.Accounts.Where(p => p.Id == comment.AccountId).FirstOrDefaultAsync();
            var cvw = new CommentViewModel()
            {
                CommentContent = comment.CommentContent,
                CommentTime = DateTime.Now.Subtract(comment.DateCreated).TotalHours,
                CommentId = comment.CommentId
            };

            cvw.AccountType = acc.Result.AccountType;
            cvw.FirstName = acc.Result.FirstName;
            cvw.LastName = acc.Result.LastName;
            cvw.Email = acc.Result.Email;
            cvw.Account = acc.Result;
            retList.Add(cvw);
        }

        return retList;
    }

}