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

           int People = dbContext.People.ToList().Count();
            ViewBag.People = People;
           
           int Public_Organizations = dbContext.Public_Organizations.ToList().Count();
            ViewBag.Public_Organizations = Public_Organizations;


            return View();
        }
    }
}
