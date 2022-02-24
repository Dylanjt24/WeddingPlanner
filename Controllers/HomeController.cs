using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private WeddingPlannerContext db;
        public HomeController(WeddingPlannerContext context)
        {
            db = context;
        }

        public int? uid
        {
            get { return HttpContext.Session.GetInt32("Uid");}
        }

        public bool IsLoggedIn
        {
            get { return uid != null;}
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            if (IsLoggedIn)
                return RedirectToAction("Index", "Weddings");
            return View();
        }

        [HttpPost("/register")]
        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid)
                if (db.Users.Any(u => u.Email == newUser.Email))
                    ModelState.AddModelError("Email", "is already taken");

            if (!ModelState.IsValid)
                return View("Index");
            
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            newUser.Password = hasher.HashPassword(newUser, newUser.Password);

            db.Add(newUser);
            db.SaveChanges();
            HttpContext.Session.SetInt32("Uid", newUser.UserId);
            HttpContext.Session.SetString("FullName", newUser.FullName());

            return RedirectToAction("Index", "Weddings");
        }

        [HttpPost("/login")]
        public IActionResult Login(LoginUser loginUser)
        {
            User dbUser = db.Users.FirstOrDefault(u => u.Email == loginUser.LoginEmail);
            if (dbUser == null)
                ModelState.AddModelError("LoginEmail", "Incorrect email/password");

            if (!ModelState.IsValid)
                return View("Index");
            
            PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
            PasswordVerificationResult pwCompareResult = hasher.VerifyHashedPassword(loginUser, dbUser.Password, loginUser.LoginPassword);
            if (pwCompareResult == 0)
            {
                ModelState.AddModelError("LoginEmail", "Incorrect email/password");
                return View("Index");
            }

            HttpContext.Session.SetInt32("Uid", dbUser.UserId);
            HttpContext.Session.SetString("FullName", dbUser.FullName());
            return RedirectToAction("Index", "Weddings");
        }
        
        [HttpPost("/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
