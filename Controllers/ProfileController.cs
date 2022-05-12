using LinkedHUCENGv2.Data;
using Microsoft.AspNetCore.Mvc;
using LinkedHUCENGv2.Models;
using static LinkedHUCENGv2.Utils.PostUtils;
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

        var posts = await GetPostsOfUser(account, _context);
        var postModels = SortPosts(posts.Select(GeneratePostViewModel).ToList());
        var tuple = new Tuple<UserProfileModel, List<PostViewModel>>(viewedUser, postModels);
        return View(tuple);
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