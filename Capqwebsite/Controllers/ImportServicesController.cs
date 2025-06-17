using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capqwebsite.Controllers
{
    public class ImportServicesController : Controller
    {
        [AllowAnonymous]

        [Route("/ImportServices/Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
