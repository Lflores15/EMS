using EMS.Models;
using EMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;  // Add logger
using System.Linq;
using System.Threading.Tasks;

namespace EMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;  // Logger for debugging

        public AccountController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;  // Initialize logger
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
                // Check if the email already exists in the system
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    // Add an error to the model state if the email already exists
                    ModelState.AddModelError("Email", "An account with this email already exists.");
                    return View(model);  // Return the view with the error message
                }

                // Create a new user if no existing user is found
                var newUser = new User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    IsApproved = false // Automatically denies new user 
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    // Save user info in the session
                    HttpContext.Session.SetString("Email", newUser.Email);
                    HttpContext.Session.SetString("Username", newUser.UserName);

                    // Add the user to the "User" role
                    await _userManager.AddToRoleAsync(newUser, "User");

                    _logger.LogInformation("User registered successfully: {Email}", newUser.Email);

                    return RedirectToAction("SignupSuccessful");
                }

                // Add any other errors from the user creation attempt
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);  // Return the view with validation errors
        }

        // GET: /Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Home/Dashboard");  // Default redirect to Dashboard
            ViewData["ReturnUrl"] = returnUrl;  // Pass returnUrl to the view to handle redirects after login
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Check if the user is approved
                    if (user.IsApproved)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                        if (result.Succeeded)
                        {
                            _logger.LogInformation("User logged in: {Email}", model.Email);  // Log login success
                            // Store session data on successful login
                            HttpContext.Session.SetString("Email", user.Email);
                            HttpContext.Session.SetString("Username", user.UserName);

                            return Redirect(returnUrl ?? "~/Home/Dashboard");  // Redirect to returnUrl or default Dashboard
                        }
                        else
                        {
                            _logger.LogWarning("Login failed for {Email}: Invalid credentials", model.Email);  // Log login failure reason
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Login failed for {Email}: Account not approved", model.Email);  // Log account not approved
                        ModelState.AddModelError(string.Empty, "Your account is not approved yet.");
                    }
                }
                else
                {
                    _logger.LogWarning("Login failed: User not found for {Email}", model.Email);  // Log user not found
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            return View(model);  // Return the view with validation errors
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
            HttpContext.Session.Clear();  // Clear session on logout
            Response.Cookies.Delete(".AspNetCore.Identity.Application");

            _logger.LogInformation("User logged out successfully.");

            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccountDetails
        public async Task<IActionResult> AccountDetails()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewData["Roles"] = string.Join(", ", roles);  // Join roles into a comma-separated string to display

            return View(user);  // Pass the user to the view as well
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
