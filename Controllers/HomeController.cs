using System.Diagnostics;
using LinkedHUCENGv2.Data;
using Microsoft.AspNetCore.Mvc;
using LinkedHUCENGv2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LinkedHUCENGv2.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public HomeController(ILogger<HomeController> logger,
                          ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _logger = logger;
        _context = context;
        _hostEnvironment = hostEnvironment;
    }
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (!_context.Accounts.Any())
            return View();
        if (!User.Identity.IsAuthenticated) return View();
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        return currAcc.IsAdmin ? RedirectToAction("Index", "Admin") : RedirectToAction("Homepage");
    }

    public async Task<IActionResult> Homepage()
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Login", "Account");
        var followControl = new FollowController(_context);
        var userProfileModel = new UserProfileModel
        {
            Id = currAcc.Id,
            FirstName = currAcc.FirstName,
            LastName = currAcc.LastName,
            ProfileBio = currAcc.ProfileBio,
            Phone = currAcc.Phone,
            Url = currAcc.Url,
            ProfilePhoto = currAcc.ProfilePhoto,
            FollowersCount = followControl.GetFollowerCount(currAcc.Id),
            FollowingCount = followControl.GetFollowingCount(currAcc.Id),
            StudentNumber = currAcc.StudentNumber,
            AccountType = currAcc.AccountType,
            Email = currAcc.Email
        };
        var posts = await _context.Post.Where(p => p.Poster.Email == User.Identity.Name).ToListAsync();
        var postModels = posts.Select(post => new PostViewModel
        {
            PosterAccount = currAcc,
            PostContent = post.PostContent,
            PostTime = DateTime.Now.Subtract(post.PostTime).Minutes,
            PostId = post.PostId,
            AccountType = currAcc.AccountType,
            FirstName = currAcc.FirstName,
            LastName = currAcc.LastName,
            PosterId = currAcc.Id,
            PostType = post.PostType
        })
            .ToList();
        ViewBag.color1 = "#CBCBCB";
        ViewBag.color2 = "#8000FF";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "#240046";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "none";
        ViewBag.leftInside = "none";
        ViewBag.accountForViewBag = userProfileModel;
        var tuple = new Tuple<UserProfileModel, List<PostViewModel>>(userProfileModel, postModels);
        return View(tuple);
    }
    public async Task<IActionResult> Settings()
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
    .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Login", "Account");
        var followControl = new FollowController(_context);
        var userProfileModel = new UserProfileModel
        {
            Id = currAcc.Id,
            FirstName = currAcc.FirstName,
            LastName = currAcc.LastName,
            ProfileBio = currAcc.ProfileBio,
            Phone = currAcc.Phone,
            Url = currAcc.Url,
            ProfilePhoto = currAcc.ProfilePhoto,
            FollowersCount = followControl.GetFollowerCount(currAcc.Id),
            FollowingCount = followControl.GetFollowingCount(currAcc.Id),
            StudentNumber = currAcc.StudentNumber
        };
        ViewBag.color1 = "#CBCBCB";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#8000FF";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "#240046";
        ViewBag.left = "block";
        ViewBag.leftInside = "block";
        ViewBag.accountForViewBag = userProfileModel;
        return View("~/Views/Home/Settings.cshtml");
    }
    public async Task<IActionResult> Notifications()
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
    .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Login", "Account");
        var followControl = new FollowController(_context);
        var userProfileModel = new UserProfileModel
        {
            Id = currAcc.Id,
            FirstName = currAcc.FirstName,
            LastName = currAcc.LastName,
            ProfileBio = currAcc.ProfileBio,
            Phone = currAcc.Phone,
            Url = currAcc.Url,
            ProfilePhoto = currAcc.ProfilePhoto,
            FollowersCount = followControl.GetFollowerCount(currAcc.Id),
            FollowingCount = followControl.GetFollowingCount(currAcc.Id),
            StudentNumber = currAcc.StudentNumber
        };
        ViewBag.color1 = "#CBCBCB";
        ViewBag.color2 = "#CBCBCB";
        ViewBag.color3 = "#CBCBCB";
        ViewBag.colorBG1 = "none";
        ViewBag.colorBG2 = "none";
        ViewBag.colorBG3 = "none";
        ViewBag.left = "block";
        ViewBag.leftInside = "block";
        ViewBag.accountForViewBag = userProfileModel;
        return View("~/Views/Home/Notifications.cshtml");
    }





    [HttpPost]
    public async Task<IActionResult> Edit(string id, [Bind("FirstName,LastName,Phone,Url, ProfileBio,ProfilePhoto,ProfilePhotoFile")] Account account)
    {
        var user = await _context.Accounts.FindAsync(id);
        if (user is null)
            return NotFound();
        if (id != user.Id)
            return NotFound();

        ModelState.Remove("AccountType");
        if (ModelState.IsValid)
        {
            var filePath = Path.Combine(_hostEnvironment.WebRootPath, "img");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            if (account.ProfilePhotoFile != null)
            {
                var fullFileName = Path.Combine(filePath, account.ProfilePhotoFile.FileName);
                //upload file
                using (var fileStream = new FileStream(fullFileName, FileMode.Create))
                {
                    await account.ProfilePhotoFile.CopyToAsync(fileStream);
                    user.ProfilePhoto = account.ProfilePhotoFile.FileName;
                }
            }
            user.FirstName = account.FirstName;
            user.LastName = account.LastName;
            user.Phone = account.Phone;
            user.Url = account.Url;
            user.ProfileBio = account.ProfileBio;

            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Homepage");
        }
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> RequestUserData()
    {
        var currAcc = await _context.Accounts.Where(m => m.Email == User.Identity.Name)
            .FirstOrDefaultAsync();
        if (currAcc is null)
            return RedirectToAction("Login", "Account");
        var notification = new Notification
        {
            NotificationType = "request",
            IsRead = false,
            NotificationTime = DateTime.Now,
            NotificationContent = currAcc.FirstName + " " + currAcc.LastName + " has requested user information."
        };
        var admin = _context.Accounts.Where(u => u.IsAdmin).ToList().FirstOrDefault();
        if (admin is null) return NotFound();
        admin.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return RedirectToAction("Homepage", "Home");
    }



    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}