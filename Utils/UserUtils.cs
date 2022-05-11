using LinkedHUCENGv2.Controllers;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using LinkedHUCENGv2.Models.UserViewModels;

namespace LinkedHUCENGv2.Utils;

public class UserUtils 
{
    public static string GetFullName(Account account)
    {
        return account.FirstName + " " + account.LastName;
    }

    public static UserProfileModel GenerateUserProfileModel(Account account, ApplicationDbContext context)
    {
        var followControl = new FollowController(context);
        var userProfileModel = new UserProfileModel
        {
            Id = account.Id,
            FirstName = account.FirstName,
            LastName = account.LastName,
            ProfileBio = account.ProfileBio,
            Phone = account.Phone,
            Url = account.Url,
            ProfilePhoto = account.ProfilePhoto,
            FollowersCount = followControl.GetFollowerCount(account.Id),
            FollowingCount = followControl.GetFollowingCount(account.Id),
            StudentNumber = account.StudentNumber,
            AccountType = account.AccountType,
            Email = account.Email,
        };
        return userProfileModel;

    }
}