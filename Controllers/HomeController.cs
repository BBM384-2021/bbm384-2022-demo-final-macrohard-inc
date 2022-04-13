using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LinkedHUCENGv2.Models;
using Microsoft.AspNetCore.Authorization;

namespace LinkedHUCENGv2.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    
    [AllowAnonymous]
    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated)
            return RedirectToAction("Homepage");
        
        return View();
    }

    public IActionResult Homepage()
    {
        return View();
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
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}