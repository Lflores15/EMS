using Microsoft.AspNetCore.Mvc;

namespace EMS.Controllers
{
    public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        // Make sure to check if the user is logged in here
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}

    }

