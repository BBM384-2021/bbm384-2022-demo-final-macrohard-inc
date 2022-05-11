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

    public static PostViewModel GeneratePostViewModel(Post post)
    {
        var account = post.Poster;
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
            Comments = CreateCommentViews(post.Comments),
            LikeCount = post.Likes.Count
        };
        return postViewModel;
    }
    
    public static List<CommentViewModel> CreateCommentViews(List<Comment> comments)
    {
        var retList = new List<CommentViewModel>();
        if (comments.Count == 0)
        {
            return retList;
        }

        foreach (var comment in comments)
        {
            var cvw = new CommentViewModel()
            {
                CommentContent = comment.CommentContent,
                CommentTime = DateTime.Now.Subtract(comment.DateCreated).TotalHours,
                CommentId = comment.CommentId
            };
            if (comment.Account is null)
            {
                continue;
            }
            cvw.AccountType = comment.Account.AccountType;
            cvw.FirstName = comment.Account.FirstName;
            cvw.LastName = comment.Account.LastName;
            cvw.Email = comment.Account.Email;
            cvw.Account = comment.Account;
            retList.Add(cvw);
        }

        return retList;
    }
}