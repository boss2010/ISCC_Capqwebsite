using EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace Capqwebsite.Controllers
{
    public class FarmController : Controller
    {
        [AllowAnonymous]
        [Route("/Farm/Index")]
        public IActionResult Index(string FarmCode)
        {
            AgricultureDBContext dbContext = new AgricultureDBContext();

            var FarmDate = dbContext.FarmStops.Where(a => a.Farmcode == FarmCode).Select(a=> new FarmStopVM
            {
                Id=a.Id,
                StopDate = a.StopDate,
                Farmcode = a.Farmcode,
                Cropname = a.Cropname,
                Previewdate = a.Previewdate,
                Compname = a.Compname,
                Farmname = a.Farmname,
                Text104 = a.Text104,
            }).ToList();


            return View(FarmDate);
        }
    }
}
