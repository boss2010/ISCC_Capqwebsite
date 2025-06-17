using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capqwebsite.Controllers
{
    public class ExportServicesController : Controller
    {
        [AllowAnonymous]

        [Route("/ExportServices/Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
