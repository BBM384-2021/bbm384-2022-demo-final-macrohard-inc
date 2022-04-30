namespace LinkedHUCENGv2.Models;

public class UserProfileModel
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileBio { get; set; }
    public string? Phone { get; set; }
    public string? Url { get; set; }
    public string? ProfilePhoto { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public string? StudentNumber { get; set; }
    public string? FollowStatus { get; set; }

}