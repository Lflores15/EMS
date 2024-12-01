using EMS.Data;
using EMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Registration model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email is already used
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "An account with this email already exists.");
                    return View(model);
                }

                var newUser = new User
                {
                    UserName = model.Username, // Use the provided Username
                    Email = model.Email, // Use email for notifications, etc.
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    // Store the username and email in the session
                    HttpContext.Session.SetString("Email", newUser.Email);
                    HttpContext.Session.SetString("Username", newUser.UserName);

                    // Optionally, add the user to a default role (e.g., "User")
                    await _userManager.AddToRoleAsync(newUser, "User");

                    return RedirectToAction("SignupSuccessful");
                }

                // Handle registration errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Please enter both email and password.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                // Sign in the user
                var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Store Username and Email in session
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetString("Username", user.UserName);

                    return RedirectToAction("LoginSuccessful");
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View();
        }

        // GET: /Account/LoginSuccessful
        public IActionResult LoginSuccessful()
        {
            return View();
        }

        // GET: /Account/SignupSuccess
        public IActionResult SignupSuccessful()
        {
            return View();
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear(); // Clear session on logout
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccountDetails
        public async Task<IActionResult> AccountDetails()
        {
            var username = HttpContext.Session.GetString("Username"); // Retrieve the username from session
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Retrieve the roles for the user
            var roles = await _userManager.GetRolesAsync(user);
            ViewData["Roles"] = string.Join(", ", roles); // Pass the roles to the view

            return View(user);
        }

        // Ensure the roles are created at startup if they don't exist
        private async Task EnsureRolesAsync()
        {
            var roleExist = await _roleManager.RoleExistsAsync("User");
            if (!roleExist)
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }
        }
    }
}
