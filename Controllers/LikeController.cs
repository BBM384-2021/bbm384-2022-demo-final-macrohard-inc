#nullable disable
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using static LinkedHUCENGv2.Utils.UserUtils;
using static LinkedHUCENGv2.Utils.PostUtils;

namespace LinkedHUCENGv2.Controllers;
public class LikeController : Controller
{
    private readonly ApplicationDbContext _context;

    public LikeController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    [ActionName("IsLiked")]
    public async Task<bool> IsPostLikedAsync(string userId, int postId)
    {
        var like = await _context.Likes.FirstOrDefaultAsync(l => l.Account.Id == userId && l.Post.PostId == postId);
        return like != null;
    }
    public bool IsPostLiked(string userId, int postId)
    {
        var like = _context.Likes.FirstOrDefault(l => l.Account.Id == userId && l.Post.PostId == postId);
        return like != null;
    }

    [HttpGet]
    [ActionName("Likes")]
    public async Task<IActionResult> GetLikeList(int postId)
    {
        var likes = await _context.Likes.Where(l => l.Post.PostId == postId).Include(l => l.Account).ToListAsync();
        var accounts = likes.Select(like => like.Account).ToList(); 
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        ViewBag.UserName = GetFullName(currAcc);
        ViewBag.color1 = "#CBCBCB";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "block";
        ViewBag.leftInside = "block";
        ViewBag.accountForViewBag = GenerateUserProfileModel(currAcc, _context);;
        return View("ListAccounts", accounts);
    }
    
    [HttpPost]
    public async Task<IActionResult> LikePost(int postId)
    {
        Console.WriteLine(postId);
        var post = await _context.Post.FirstOrDefaultAsync(p => p.PostId == postId);
        var currUser = await _context.Accounts.FirstOrDefaultAsync(m => m.Email == User.Identity.Name);
        if (post is null)
            return Json(-1);
        if (currUser is null)
            return Json(-1);

        if (await IsPostLikedAsync(currUser.Id, postId))
        {
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.Account.Id == currUser.Id && l.Post.PostId == postId);
            post.Likes.Remove(like);
            _context.Post.Update(post);
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return Json(0);
        }
        else
        {
            var like = new Like
            {
                Account = currUser,
                Post = post,
                DateCreated = DateTime.Now
            };
            post.Likes.Add(like);
            _context.Post.Update(post);
            _context.Add(like);
            var notifyController = new NotificationController(_context);
            notifyController.CreateLikeNotification(currUser, post);
            await _context.SaveChangesAsync();
            return Json(1);
        }
    }
    
}