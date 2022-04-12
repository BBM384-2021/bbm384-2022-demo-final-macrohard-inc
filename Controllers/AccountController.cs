using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Newtonsoft.Json;  
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
            if (!String.IsNullOrEmpty(account.Email) && !String.IsNullOrEmpty(account.Password))
            {
                // check emails to see if the user is registered before
                var user = _context.Accounts.Where(s => s.Email.Equals(account.Email)).ToList();
                if (user.Any() && user[0].Password != account.Password) // if the user has entered a wrong password
                {
                    ViewBag.Text = "Password is Incorrect";
                }
                // if the user is not registered or the email entered is not correct
                else if (!user.Any())
                {
                    ViewBag.Text = "You are not registered or you did not enter your email correctly.";
                }
                if (user.Any() && user[0].Password == account.Password)
                {
                    if (user[0].IsAdmin)
                    {
                        return new AdminController().ListUser(); 
                    }
                    return View("~/Views/homepage.cshtml"); // some user view - to be updated later
                }
            }
            return View("~/Views/Home/Login.cshtml");
        }

        [HttpPost]
        public JsonResult Follow(int account1Id, int account2Id)
        {
            var account1 = _context.Accounts.Find(account1Id);
            var account2 = _context.Accounts.Find(account2Id);
            IDictionary<int, int> returnDict = new Dictionary<int, int>();
            if (account1 != null && account2 != null && account1Id != account2Id)
            {
                var existingFollow = _context.Follows.FirstOrDefault(f => f.Account1Id == account1Id && f.Account2Id == account2Id);
                if (existingFollow != null)
                {
                    _context.Follows.Remove(existingFollow);
                    _context.SaveChanges();
                    return Json(returnDict);
                }
                
                var newFollow = new Follow
                {
                    Account1 = account1,
                    Account2 = account2,
                    DateCreated = DateTime.UtcNow
                };
                // Add to context
                _context.Follows.Add(newFollow);
                _context.SaveChanges();
                // Returns JSON {followerID: followingID}
                returnDict.Add(account1Id, account2Id);
            }
            return Json(returnDict);
        }
        
    }
}