using Microsoft.AspNetCore.Mvc;
using EMS.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace EMS.Controllers
{
    public class AccountController : Controller
    {
        // In-memory "database" for users (in production, you'd store this in a real database)
        private static List<User> users = new List<User>();

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(Registration model)
        {
            // Check if the passwords match
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            // Check if the email already exists
            if (users.Exists(u => u.Email == model.Email))
            {
                ViewBag.Error = "Email already exists.";
                return View();
            }

            // Hash the password for security purposes
            string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: model.Password,
                salt: Encoding.UTF8.GetBytes(model.Email),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Split full name into first and last names (assuming space separation)
            var nameParts = model.FullName.Split(' ');
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            // Create a new user and add to the "database"
            var newUser = new User
            {
                Email = model.Email,
                PasswordHash = passwordHash,
                Role = "User", // Default role for new users
                Name = model.FullName // Store full name
            };
            users.Add(newUser);

            // Set session variable for the logged-in user
            HttpContext.Session.SetString("Email", model.Email);

            // Redirect to the Home page or another page
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Find the user by email
            var user = users.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                // Verify the password hash
                string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Encoding.UTF8.GetBytes(email),
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                if (user.PasswordHash == hashedPassword)
                {
                    // Set session variable for the logged-in user
                    HttpContext.Session.SetString("Email", email);
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Invalid login attempt.";
            return View();
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            // Clear session on logout
            HttpContext.Session.Remove("Email");
            return RedirectToAction("Index", "Home");
        }
    }
}
