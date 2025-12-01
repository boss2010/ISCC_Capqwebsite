using EF.Models;
using Microsoft.AspNetCore.Mvc;

namespace Capqwebsite.Controllers
{
    public class dashBoardController : Controller
    {
        public IActionResult Index()
        {

            AgricultureDBContext dbContext=new AgricultureDBContext();

            
            return View(dbContext.WebsiteTypeDetails.ToList());
        }
        public IActionResult dash()
        {

            AgricultureDBContext dbContext = new AgricultureDBContext();


            return View(dbContext.WebsiteTypeDetails.ToList());
        }
    }
}
