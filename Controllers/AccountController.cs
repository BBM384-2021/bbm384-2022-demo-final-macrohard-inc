using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using LinkedHU_CENG.Models;
using Microsoft.AspNetCore.Mvc;
using LinkedHU_CENG.Data;
using LinkedHU_CENG.Models;
using LinkedHU_CENG.Utils;

namespace LinkedHU_CENG.Controllers
{
    public class AccountController : Controller
    {
        private readonly Context _context;
        public AccountController(Context context)
        {
            _context = context;
        }

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
            var rawPass = account.Password;
            var salt = EncryptPassword.GenerateSalt();
            newUser.Salt = salt;
            newUser.Password = EncryptPassword.ComputeHash(Encoding.UTF8.GetBytes(rawPass), Encoding.UTF8.GetBytes(salt));
            newUser.FirstName = account.FirstName;
            newUser.LastName = account.LastName;
            newUser.AccountType = account.AccountType;
            newUser.Email = account.Email; // problem showing error messages for invalid email combinations
            newUser.IsAdmin = false;
            if (ModelState.IsValid)
            {
                _context.Accounts.Add(newUser);
                _context.SaveChanges();
                return View("~/Views/homepage.cshtml");

            }
            else
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y=>y.Count>0)
                    .ToList();
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
                if (user.Any() && ValidatePass(user[0].Password, user[0])) // if the user has entered a wrong password
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
                    return View("~/Views/homepage.cshtml"); // some user view - to be updated later
                }
            }
            return View("~/Views/Home/Login.cshtml");
            
        }

        private bool ValidatePass(string enteredPass, Account account)
        {
            var salt = account.Salt;
            var hashedPass = account.Password;

            var checkPass = EncryptPassword.ComputeHash(Encoding.UTF8.GetBytes(enteredPass), Encoding.UTF8.GetBytes(salt));
            return checkPass == hashedPass;
        }
    }
}