using Microsoft.AspNetCore.Mvc;

namespace Capqwebsite.Controllers
{
    public class AdvertisementsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
