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
        var currAcc = await _context.Accounts.FirstOrDefaultAsync(m => m.Email == User.Identity.Name);
        var post = await _context.Post.FirstOrDefaultAsync(p => p.PostId == postId);
        if (currAcc is null)
            return Json("currAcc is null");
        if (post is null)
            return Json("post is null");
        var comment = new Comment
        {
            Account = currAcc,
            Post = post,
            CommentContent = commentContent,
            DateCreated = DateTime.Now
        };
        currAcc.Comments.Add(comment);
        post.Comments.Add(comment);
        _context.Add(comment);
        await _context.SaveChangesAsync();
        return Redirect("/Post/Feed");
    } 
}