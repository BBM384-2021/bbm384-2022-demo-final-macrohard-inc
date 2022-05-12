#nullable disable
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;

namespace LinkedHUCENGv2.Controllers;
public class CommentController : Controller
{
    private readonly ApplicationDbContext _context;

    public CommentController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateComment(int postId, string commentContent)
    {
        var currAcc = await _context.Accounts.Include(a => a.Comments)
            .FirstOrDefaultAsync(m => m.Email == User.Identity.Name);
        var post = await _context.Post.Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.PostId == postId);
        if (currAcc is null)
            return Json("currAcc is null");
        if (post is null)
            return Json("post is null");
        if (commentContent is null)
            return Redirect("/Post/Feed");
        var comment = new Comment
        {
            AccountId = currAcc.Id,
            Account = currAcc,
            Post = post,
            CommentContent = commentContent,
            DateCreated = DateTime.Now
        };
        
        currAcc.Comments.Add(comment);
        post.Comments.Add(comment);
        _context.Add(comment);
        _context.Update(post);
        _context.Update(currAcc);
        var notifyController = new NotificationController(_context);
        notifyController.CreateCommentNotification(currAcc, post);
        await _context.SaveChangesAsync();
        return await Task.Run(() => Redirect("/Post/Feed"));
    }
    [HttpPost]
    public async Task<IActionResult> EditComment(int commentId, string commentContent)
    {
        var currAcc = await _context.Accounts.FirstOrDefaultAsync(m => m.Email == User.Identity.Name);
        if (currAcc is null)
            return Json(0);
        var comment = await _context.Comments.FirstOrDefaultAsync(p => p.CommentId == commentId);
        if (comment is null)
            return Json(0);
        comment.CommentContent = commentContent;
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
        return Json(1);

    }

    [HttpPost]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var currAcc = await _context.Accounts.FirstOrDefaultAsync(m => m.Email == User.Identity.Name);
        if (currAcc is null)
            return Json("currAcc is null");
        var comment = await _context.Comments.FirstOrDefaultAsync(p => p.CommentId == commentId);
        if (comment is null)
            return Json("comment is null");
        currAcc.Comments.Remove(comment);
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return Redirect("/Post/Feed");

    }
}