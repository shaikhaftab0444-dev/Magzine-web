using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Digital_Magzine.Controllers
{
    [AllowAnonymous]
    public class LegalController : Controller
    {
        // GET: /Legal/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Legal/Terms
        public IActionResult Terms()
        {
            return View();
        }

        // GET: /Legal/Cookies
        public IActionResult Cookies()
        {
            return View();
        }
    }
}
