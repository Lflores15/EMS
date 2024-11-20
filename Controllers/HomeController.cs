using Microsoft.AspNetCore.Mvc;

namespace EMS.Controllers
{
    public class HomeController : Controller
    {
        // Action for the homepage
        public IActionResult Index()
        {
            return View();
        }

        // Action for the privacy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Action for the error page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
