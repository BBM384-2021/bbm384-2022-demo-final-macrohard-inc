using LinkedHUCENGv2.Data;
using Microsoft.AspNetCore.Mvc;
using LinkedHUCENGv2.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkedHUCENGv2.Controllers;

public class ProfileController : Controller
{
    private readonly ILogger<ProfileController> _logger;
    private readonly ApplicationDbContext _context;

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
    public async Task<IActionResult> ViewProfile(string? id)
    {
        var viewerAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        var account = await _context.Accounts.Where(a => a.Id == id).FirstOrDefaultAsync();
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
            FollowingCount = followControl.GetFollowingCount(account.Id),
    
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
            FollowersCount = followControl.GetFollowerCount(currentAccount.Id),
            FollowingCount = followControl.GetFollowingCount(currentAccount.Id),
        };
        
        ViewBag.color1 = "#CBCBCB";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "none";
        ViewBag.leftInside = "none";
        ViewBag.accountForViewBag = currUserProfileModel;
        ViewBag.followColor = "white";
        ViewBag.followBColor = "#240046";
        ViewBag.FollowText = "Follow";
        if (followControl.IsUserFollowed(viewerAcc.Id, id))
        {
            ViewBag.followColor = "gray";
            ViewBag.followBColor = "#F4F1F7";
            ViewBag.FollowText = "Unfollow";
        }
        var posts = await _context.Post.Include(p => p.Likes).Include(p => p.Comments).Include(p => p.Images).Include(p => p.PDFs).Where(p => p.Poster.Id == account.Id).ToListAsync();
        var postModels = posts.Select(post => new PostViewModel
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
            Comments = CreateCommentViews(post.Comments),
            LikeCount = post.Likes.Count
        }).ToList();
        var tuple = new Tuple<UserProfileModel, List<PostViewModel>>(viewedUser, postModels);
        return View(tuple);
    }
    
    private List<CommentViewModel> CreateCommentViews(List<Comment> comments)
    {
        return comments.Select(comment => new CommentViewModel
            {
                CommentContent = comment.CommentContent,
                CommentTime = DateTime.Now.Subtract(comment.DateCreated).TotalHours,
                CommentId = comment.CommentId,
                AccountType = comment.Account.AccountType,
                FirstName = comment.Account.FirstName,
                LastName = comment.Account.LastName,
                Email = comment.Account.Email,
                Account = comment.Account
            })
            .ToList();
    }

}