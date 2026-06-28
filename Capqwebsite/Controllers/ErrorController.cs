using Microsoft.AspNetCore.Mvc;

namespace Capqwebsite.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{code}")]
        public IActionResult HandleError(int code)
        {
            if (code == 404)
                return View("NotFound");

            return View("Error");
        }
    }
}
