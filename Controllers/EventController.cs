using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.Data;
using EMS.Models;

namespace EMS.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events - All authenticated users can access this
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.ToListAsync();
            return View(events);
        }

        // GET: Events/Details/5 - All authenticated users can view event details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Check if the user can edit the event (admin or organizer)
            var userRole = User.IsInRole("Admin") || User.Identity.Name == eventItem.Organizer;
            ViewBag.CanEdit = userRole;  // Pass this to the view to determine if the user can edit

            return View(eventItem);
        }

        // GET: Events/Create - Only logged-in users can create events
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create - Only logged-in users can create events
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Date,Location,Organizer")] Event eventItem)
        {
            if (ModelState.IsValid)
            {
                eventItem.Organizer = User.Identity.Name; // Set the organizer to the current signed-in user
                _context.Add(eventItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eventItem);
        }

        // GET: Events/Edit/5 - Only the event organizer or admin can edit
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            // Ensure only the organizer or admin can edit
            if (User.Identity.Name != eventItem.Organizer && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Date,Location,Organizer,IsConfirmed")] Event eventItem)
        {
            if (id != eventItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure the user is authorized to edit the event
                    if (User.Identity.Name != eventItem.Organizer && !User.IsInRole("Admin"))
                    {
                        return Forbid();
                    }

                    // Save the updated event in the database
                    _context.Update(eventItem);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eventItem);
        }


        // Only the event organizer or admin can delete
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventItem == null)
            {
                return NotFound();
            }

            // Ensure only the organizer or admin can delete
            if (User.Identity.Name != eventItem.Organizer && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(eventItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }

        [Authorize]
        public IActionResult Calendar()
        {
            var events = _context.Events
                .Where(e => e.IsConfirmed == true) // Ensure only confirmed events
                .ToList();

            // Debugging: Log the retrieved events
            //Console.WriteLine($"Retrieved {events.Count} confirmed events:");
            //foreach (var ev in events)
            //{
            //Console.WriteLine($"Event: {ev.Name}, Date: {ev.Date}, Organizer: {ev.Organizer}");
            //}

            return View(events);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmEvent(int id, bool isConfirmed)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            // Only allow confirming if the event is not already confirmed
            if (!eventItem.IsConfirmed)
            {
                eventItem.IsConfirmed = isConfirmed;
                _context.Update(eventItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = isConfirmed ? "Event confirmed." : "Event denied.";
            }
            else
            {
                TempData["Error"] = "This event is already confirmed and cannot be updated.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Sorting of list page 
        [HttpGet]
        [Route("Events/Index")]
        public async Task<IActionResult> Index(string sortBy, string sortOrder)
        {
            // Default values for sortBy and sortOrder
            if (string.IsNullOrEmpty(sortBy)) sortBy = "Name";
            if (string.IsNullOrEmpty(sortOrder)) sortOrder = "asc";

            var events = _context.Events.Where(e => e.IsConfirmed == true).AsQueryable();

            switch (sortBy.ToLower())
            {
                case "name":
                    events = sortOrder == "asc" ? events.OrderBy(e => e.Name) : events.OrderByDescending(e => e.Name);
                    break;
                case "description":
                    events = sortOrder == "asc" ? events.OrderBy(e => e.Description) : events.OrderByDescending(e => e.Description);
                    break;
                case "date":
                    events = sortOrder == "asc" ? events.OrderBy(e => e.Date) : events.OrderByDescending(e => e.Date);
                    break;
                case "location":
                    events = sortOrder == "asc" ? events.OrderBy(e => e.Location) : events.OrderByDescending(e => e.Location);
                    break;
                case "organizer":
                    events = sortOrder == "asc" ? events.OrderBy(e => e.Organizer) : events.OrderByDescending(e => e.Organizer);
                    break;
                default:
                    events = events.OrderBy(e => e.Name);
                    break;
            }

            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;

            return View(await events.ToListAsync());
        }


    }
}
