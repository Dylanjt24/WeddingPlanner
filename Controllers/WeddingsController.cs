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
using Microsoft.EntityFrameworkCore;

namespace WeddingPlanner.Controllers
{
    public class WeddingsController : Controller
    {
        private readonly ILogger<WeddingsController> _logger;

        private WeddingPlannerContext db;
        public WeddingsController(WeddingPlannerContext context)
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

        [HttpGet("/weddings")]
        public IActionResult Index()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Index", "Home");
            ViewBag.AllWeddings = db.Weddings
            .Include(w => w.Creator)
            .Include(w => w.WeddingGuests)
            .ToList();

            return View();
        }

        [HttpGet("/weddings/new")]
        public IActionResult New()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost("/weddings/create")]
        public IActionResult Create(Wedding newWedding)
        {
            if (!ModelState.IsValid)
                return View("New");
            
            newWedding.UserId = (int)HttpContext.Session.GetInt32("Uid");
            db.Add(newWedding);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet("/weddings/{weddingId:int}")]
        public IActionResult Show(int weddingId)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Index", "Home");
            Wedding wedding = db.Weddings
            .Include(w => w.WeddingGuests)
            .ThenInclude(rsvp => rsvp.User)
            .FirstOrDefault(w => w.WeddingId == weddingId);
            if (wedding == null)
                return RedirectToAction("Index");
            return View(wedding);
        }

        [HttpPost("/weddings/{weddingId:int}/rsvp")]
        public IActionResult Rsvp(int weddingId)
        {
            Rsvp hasRsvp = db.Rsvps
            .FirstOrDefault(r => r.UserId == (int)uid && r.WeddingId == weddingId);

            if (hasRsvp == null)
            {
                Rsvp newRsvp = new Rsvp
                {
                    WeddingId = weddingId,
                    UserId = (int)uid
                };
                db.Rsvps.Add(newRsvp);
            }
            else
            {
                db.Rsvps.Remove(hasRsvp);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost("/weddings/{weddingId:int}/delete")]
        public IActionResult Delete(int weddingId)
        {
            Wedding wedding = db.Weddings.FirstOrDefault(w => w.WeddingId == weddingId);

            if (wedding != null)
            {
                db.Weddings.Remove(wedding);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
