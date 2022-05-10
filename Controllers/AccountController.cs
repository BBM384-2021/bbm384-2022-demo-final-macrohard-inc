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
                ProfilePhoto = "studentProfile.png",
                RegistrationDate = DateTime.Now,
                Posts = new List<Post>()
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                var notificationController = new NotificationController(_context);
                notificationController.CreateRegisterNotification(user);
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
        if (!ModelState.IsValid) return View(user);
        var result = await _signInManager.PasswordSignInAsync(user.Email, user.Password, user.RememberMe, false);

        if (result.Succeeded)
        {
            var acc = await _context.Accounts.Where(s => s.Email.Equals(user.Email)).ToListAsync();
            return acc.First().IsAdmin ? RedirectToAction("Index", "Admin") : RedirectToAction("Feed", "Post");
        }

        ViewBag.Text = "Invalid login! Check your mail and password.";
        ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
        return View(user);
    }


    // POST: Account/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(string id, [Bind("FirstName,LastName,Phone,Url")] Account account)
    {
        var user = await _context.Accounts.FindAsync(id);
        if (id != user?.Id)
        {
            return NotFound();
        }

        ModelState.Remove("AccountType");
        if (!ModelState.IsValid) return RedirectToAction("Index", "Home");
        user.FirstName = account.FirstName;
        user.LastName = account.LastName;
        user.Phone = account.Phone;
        user.Url = account.Url;
        _context.Update(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Home");
    }


    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");

    }
}