using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;
using LinkedHUCENGv2.Models.AuthViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LinkedHUCENGv2.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private ApplicationDbContext _context;

    public AccountController(UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new Account
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                AccountType = model.AccountType,
                Followers = new List<Follow>(),
                Following = new List<Follow>(),
                Url = "",
                StudentNumber = "",
                ProfileBio = "",
                ProfilePhoto = "",
                Posts = new List<Post>()
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                CreateRegisterNotification(user);
                return RedirectToAction("Feed", "Post");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");

        }
        return View(model);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel user)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(user.Email, user.Password, user.RememberMe, false);

            if (result.Succeeded)
            {
                var acc = await _context.Accounts.Where(s => s.Email.Equals(user.Email)).ToListAsync();
                if (acc.First().IsAdmin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Feed", "Post");
            }

            ViewBag.Text = "Invalid login! Check your mail and password.";
            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");

        }
        return View(user);
    }
    
    
    // POST: Account/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(string id, [Bind("FirstName,LastName,Phone,Url")] Account account)
    {
        var user = await _context.Accounts.FindAsync(id);
        if (id != user.Id)
        {
            return NotFound();
        }

        ModelState.Remove("AccountType");
        if (ModelState.IsValid)
        {
            user.FirstName = account.FirstName;
            user.LastName = account.LastName;
            user.Phone = account.Phone;
            user.Url = account.Url;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        return RedirectToAction("Index", "Home");
    }


    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");

    }
    
    public void CreateRegisterNotification(Account account)
    {
        var notification = new Notification
        {
            NotificationType = "register",
            IsRead = false,
            NotificationTime = DateTime.Now,
            NotificationContent = account.FirstName + " " + account.LastName + " has registered to the system."
        };
        var admin = _context.Accounts.Where(u => u.IsAdmin).ToList().FirstOrDefault();
        if (admin is null) return;
        admin.Notifications.Add(notification);
        _context.SaveChanges();
    }
}