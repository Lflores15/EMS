using EMS.Data;
using EMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMS.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Name,Description,Date,Location")] Event eventModel)
        {
            if (ModelState.IsValid)
            {
                // Add event to the database
                _context.Add(eventModel);
                _context.SaveChanges();
                
                // Redirect to the events index page after successful creation
                return RedirectToAction(nameof(Index)); // Or any page you want to show
            }
            return View(eventModel); // In case of error, return the same page
        }

        // GET: Events/Index - Display list of events
        public IActionResult Index()
        {
            var events = _context.Events.ToList(); // Retrieve all events from the database
            return View(events); // Pass events to the view
        }

        // GET: Events/Calendar - Display calendar view
        public IActionResult Calendar()
        {
            return View(); // Render the Calendar view
        }

        // GET: Events/GetEvents - Get event data for the calendar
        public IActionResult GetEvents()
        {
            var events = _context.Events.Select(e => new
            {
                title = e.Name,
                start = e.Date.ToString("yyyy-MM-dd"),
                location = e.Location
            }).ToList();

            return Json(events); // Return events in JSON format
        }
    }
}
