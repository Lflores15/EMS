using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.Data;
using EMS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace EMS.Controllers
{
    [Authorize]  // Ensure only authenticated users can access this controller
    [Authorize(Roles = "Admin")]  // Ensure only users with the "Admin" role can access this controller
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin - View events and users for admin
        public async Task<IActionResult> Index()
        {
            // Get list of events and users
            var events = await _context.Events.ToListAsync();
            var users = await _context.Users.ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                Events = events,
                Users = users
            };

            return View(viewModel);
        }

        // GET: Admin/Edit/5 - Admin can edit events
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

            return View(eventItem);
        }

        // POST: Admin/Edit/5 - Admin edits the event
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Admin/Delete/5 - Admin can delete events
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

            return View(eventItem);
        }

        // POST: Admin/Delete/5 - Admin confirms event deletion
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/ApproveUser/5 - Admin can approve a user
        public async Task<IActionResult> ApproveUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsApproved = true;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/RejectUser/5 - Admin can reject a user
        public async Task<IActionResult> RejectUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsApproved = false;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
