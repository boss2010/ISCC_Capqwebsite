using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capqwebsite.Controllers
{
    public class contactController : Controller
    {
        [AllowAnonymous]
        [Route("/contact/Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
