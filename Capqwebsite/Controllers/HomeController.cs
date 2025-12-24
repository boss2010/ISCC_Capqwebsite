using System.Diagnostics;
using Capqwebsite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EF.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels;

namespace Capqwebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        //rrrrr
        [AllowAnonymous]
        [Route("/Home/HomePage/Index")]
        public IActionResult Index()
        {
            
                AgricultureDBContext dBContext = new AgricultureDBContext();
                HomeVM homeVM = new HomeVM();
                /////////////////////////Slider/////////////////////////
                homeVM.Slider = (from sl in dBContext.WebsiteTypeDetails

                                 where sl.WebsitetypeID == 9
                                 && (sl.IsActive == true || sl.IsActive == null)
                                 select new ListSliderVM
                                 {
                                     ID = sl.ID,
                                     filepath = sl.filepath,
                                     User_Creation_Date = sl.User_Creation_Date
                                 }
                                ).ToList();
                /////////////////////////Intro/////////////////////////////
                homeVM.Intro = (from intro in dBContext.WebsiteTypeDetails

                                where intro.WebsitetypeID == 11 
                                && (intro.IsActive == true || intro.IsActive == null)
                                orderby intro.Date descending
                                select new ListIntroVM
                                {
                                    TitleAr = intro.TitleAr,
                                    descAr = intro.descAr,
                                    ID = intro.ID,
                                    filepath = intro.filepath,
                                    User_Creation_Date = intro.User_Creation_Date

                                }
                                ).Take(1).ToList();
                /////////////////////////News/////////////////////////////
                homeVM.News = (from ne in dBContext.WebsiteTypeDetails

                               where ne.WebsitetypeID == 7 && (ne.IsActive == true || ne.IsActive == null)
                               orderby ne.Date descending
                               select new ListNewsVM
                               {
                                   ID = ne.ID,
                                   TitleAr = ne.TitleAr,
                                   filepath = ne.filepath,
                                   //descAr=ne.descAr,
                                   descAr = ne.descAr != null ?
                               (ne.descAr.Length > 150 ? ne.descAr.Substring(0, 200) : ne.descAr)
                               : null,
                                   User_Creation_Date = ne.User_Creation_Date

                               }
                                ).Take(3).ToList();
                /////////////////////////Advertis/////////////////////////////
                homeVM.Advertis = (from adv in dBContext.WebsiteTypeDetails

                                   where adv.WebsitetypeID == 8
                                   && (adv.IsActive == true || adv.IsActive == null)
                                   orderby adv.Date descending
                                   select new ListAvertisVM
                                   {
                                       ID = adv.ID,
                                       TitleAr = adv.TitleAr,
                                       filepath = adv.filepath,
                                       descAr = adv.descAr,
                                       Date = adv.Date
                                   }
                                ).Take(3).ToList();

                ////////////////////////////Alert/////////////////////////////
                homeVM.Alert = (from aler in dBContext.WebsiteTypeDetails

                                where aler.WebsitetypeID == 10 
                                && (aler.IsActive == true || aler.IsActive == null)

                                select new ListAlertVM
                                {
                                    ID = aler.ID,
                                    TitleAr = aler.TitleAr,
                                    filepath = aler.filepath,
                                    descAr = aler.descAr,
                                    Date = aler.Date
                                }
                                ).ToList();
                /////////////////////////////////////////
                return View(homeVM);
          
        }


        public IActionResult ClearSession()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static string CutTo150(string input)
        {
            // Return null if input is null
            if (input == null) return null;

            // Return the string if it's 150 characters or less
            if (input.Length <= 150) return input;

            // Truncate to 147 characters and add ellipsis (total 150)
            return input.Substring(0, 147) + "...";
        }
    }
}
