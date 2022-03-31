using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using LinkedHU_CENG.Models;
using Microsoft.AspNetCore.Mvc;

namespace LinkedHU_CENG.Controllers
{
    public class AccountController : Controller
    {
        private readonly Context _context = new Context();

        public IActionResult RedirectToLogin()
        {
            return View("~/Views/Home/Login.cshtml");
        }

        public IActionResult RedirectToRegister()
        {
            return View("~/Views/Home/Register.cshtml");
        }

        [HttpPost]
        public IActionResult RegisterUser(Account account)
        {
            // implement popup for already registered users
            var newUser = new Account(); // create new user instance
            newUser.FirstName = account.FirstName;
            newUser.LastName = account.LastName;
            newUser.AccountType = account.AccountType;
            newUser.Email = account.Email; // problem showing error messages for invalid email combinations
            newUser.Password = account.Password;
            newUser.IsAdmin = false;
            if (ModelState.IsValid)
            {
                _context.Accounts.Add(newUser);
                _context.SaveChanges();
                return View("~/Views/homepage.cshtml");

            }
            return View("~/Views/Home/Register.cshtml");
        }

        [HttpPost]
        public IActionResult Login(Account account)
        {
            if (account.Email == "" || account.Password == "")
            {
                return View("~/Views/Home/Login.cshtml");
            }
            if (_context.Accounts.Where(s => s.Email.Equals(account.Email) && s.Password.Equals(account.Password)).Any())
            {
                return View("~/Views/homepage.cshtml");
            }
            ViewBag.visibility = "visible";
            ViewBag.backgroundColor = "white";
            ViewBag.backgroundColor2 = "rgba(0,0,0,0.1)";
            ViewBag.blur = "blur(20px)";
            ViewBag.text = "Invalid E-mail or Password";
            return View("~/Views/Home/Login.cshtml");
        }
    }
}