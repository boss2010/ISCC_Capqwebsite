using EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capqwebsite.Controllers
{
    public class AgricultureLawController : Controller
    {
        [AllowAnonymous]
        
        [Route("/AgricultureLaw/Index")]
        public IActionResult Index(int ID)
        {
            AgricultureDBContext DBContext = new AgricultureDBContext();
            var list=DBContext.WebsiteTypeDetails.Where(a => a.WebsitetypeID == ID && (a.IsActive != false)).ToList();
           string TypeAr= DBContext.Websitetypes.Where(a => a.ID==ID && (a.IsActive != false )).ToList().FirstOrDefault()?.TypeAr;

            ViewBag.TypeAr = TypeAr;
            return View(list);
        }
    }
}
