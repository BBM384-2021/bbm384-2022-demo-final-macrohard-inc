using LinkedHUCENGv2.Controllers;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkedHUCENGv2.Utils;

public class PostUtils
{
    public static async Task<List<Post>> GetPostsOfUser(Account account, ApplicationDbContext context)
    {
        var retList = await context.Post.Where(p => p.Poster.Id == account.Id)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Include(p => p.Images)
            .Include(p => p.PDFs)
            .AsSplitQuery()
            .ToListAsync();

        return retList;
    }
    
    public static List<PostViewModel> SortPosts(IEnumerable<PostViewModel> posts)
    {
        var sort = posts.OrderBy(p => p.PostTime).ToList();
        return sort;
    }
    
    public static List<CommentViewModel> SortComments(IEnumerable<CommentViewModel> comments)
    {
        var sort = comments.OrderBy(c => c.CommentTime).ToList();
        return sort;
    }
}
