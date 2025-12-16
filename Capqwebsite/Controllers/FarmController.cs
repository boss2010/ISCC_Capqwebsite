using Microsoft.AspNetCore.Mvc;

namespace Capqwebsite.Controllers
{
    public class FarmController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
