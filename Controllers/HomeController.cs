using EMS.Models; 
using EMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // For session management
using System;
using System.Linq;

namespace EMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context; // Database context

        // Constructor to inject the database context
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            // Check if the user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch upcoming events
            var upcomingEvents = _context.Events
                .Where(e => e.Date >= DateTime.Now) // Only future events
                .OrderBy(e => e.Date) // Sort by date
                .ToList();

            // Pass events to the view
            return View(upcomingEvents);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Details()
        {
            return View();
        }
    }
}
