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

        // POST: Events/Edit/5 - Only the event organizer or admin can edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Date,Location,Organizer")] Event eventItem)
        {
            if (id != eventItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure only the organizer or admin can update
                    if (User.Identity.Name != eventItem.Organizer && !User.IsInRole("Admin"))
                    {
                        return Forbid();
                    }

                    _context.Update(eventItem);
                    await _context.SaveChangesAsync();
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

        // GET: Events/Delete/5 - Only the event organizer or admin can delete
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

        // POST: Events/Delete/5 - Only the event organizer or admin can delete
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

        // New action for admin to confirm or deny events
        [Authorize(Roles = "Admin")]  // Only admins can confirm/deny events
        public async Task<IActionResult> ConfirmEvent(int id, bool isConfirmed)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            eventItem.IsConfirmed = isConfirmed;
            _context.Update(eventItem);
            await _context.SaveChangesAsync();

            TempData["Success"] = isConfirmed ? "Event confirmed." : "Event denied.";
            return RedirectToAction(nameof(Index));
        }
    }
}
