using Microsoft.AspNetCore.Mvc;

namespace LinkedHUCENGv2.Controllers;

public class ChatController : Controller
{
    public ActionResult Chat()
        {
            return View();
        }
}