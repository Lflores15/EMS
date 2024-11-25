using EMS.Data;
using EMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Linq;
using System.Text;

namespace EMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;  // Dependency injection for DbContext
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(Registration model)
        {
            // Validate if the passwords match
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            // Check if the email already exists in the database
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ViewBag.Error = "Email already exists.";
                return View();
            }

            // Hash the password
            string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: model.Password,
                salt: Encoding.UTF8.GetBytes(model.Email), // Salt from email
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Create a new user
            var newUser = new User
            {
                Email = model.Email,
                PasswordHash = passwordHash,
                Role = "User",  // Default role
                Name = model.FullName
            };

            // Save to the database
            _context.Users.Add(newUser);
            _context.SaveChanges();  // Persist changes to the database

            // Redirect to Home page after successful registration
            return RedirectToAction("SignupSuccessful");
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        // POST: /Account/Login
// POST: /Account/Login
[HttpPost]
public IActionResult Login(string email, string password)
{
    var user = _context.Users.FirstOrDefault(u => u.Email == email);

    if (user != null)
    {
        string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.UTF8.GetBytes(email),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        if (user.PasswordHash == hashedPassword)
        {
            HttpContext.Session.SetString("Email", email);
            return RedirectToAction("LoginSuccessful");
        }
    }

    ViewBag.Error = "Invalid email or password.";
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
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Email");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccountDetails
        public IActionResult AccountDetails()
        {
            string email = HttpContext.Session.GetString("Email");
            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }
    }
}
