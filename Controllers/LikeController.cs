#nullable disable
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;

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
    public async Task<bool> IsPostLiked(string userId, int postId)
    {
        var like = await _context.Likes.FirstOrDefaultAsync(l => l.Account.Id == userId && l.Post.PostId == postId);
        return like != null;
    }
    
    [HttpPost]
    public async Task<IActionResult> LikePost(int postId)
    {
        var post = await _context.Post.FirstOrDefaultAsync(p => p.PostId == postId);
        var currUser = await _context.Accounts.FirstOrDefaultAsync(m => m.Email == User.Identity.Name);
        if (post is null)
            return Json("post is null");
        if (currUser is null)
            return Json("currUser is null");

        if (await IsPostLiked(currUser.Id, postId))
        {
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.Account.Id == currUser.Id && l.Post.PostId == postId);
            post.Likes.Remove(like);
            _context.Post.Update(post);
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return Redirect("/Post/Feed");
        }
        else
        {
            var like = new Like
            {
                Account = currUser,
                Post = post,
                DateCreated = DateTime.Now
            };
            var notifyController = new NotificationController(_context);
            post.Likes.Add(like);
            _context.Post.Update(post);
            _context.Add(like);
            notifyController.CreateLikeNotification(currUser, post);
            await _context.SaveChangesAsync();
            return Redirect("/Post/Feed");
        }
    }
    
}