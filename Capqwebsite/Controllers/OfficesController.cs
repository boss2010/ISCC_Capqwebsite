using EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capqwebsite.Controllers
{
    public class OfficesController : Controller
    {
        [AllowAnonymous]
        [Route("/Offices/Index")]
        public IActionResult Index()
        {
            AgricultureDBContext DBContext = new AgricultureDBContext();
            var list=DBContext.Outlets.Where (a=>a.IsActive==true).ToList();
            return View(list);
        }
    }
}
