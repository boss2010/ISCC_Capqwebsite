using System;
using EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capqwebsite.Controllers
{
    public class NewsController : Controller
    {
        [AllowAnonymous]
        [Route("/News/Index")]
        public IActionResult Index(int ID)
        {
            AgricultureDBContext DBContext = new AgricultureDBContext();
            var list = DBContext.WebsiteTypeDetails.Where(a => a.WebsitetypeID == ID && (a.IsActive == true || a.IsActive == null)).ToList();
            string TypeAr = DBContext.Websitetypes.Where(a => a.ID == ID).ToList().FirstOrDefault()?.TypeAr;

            ViewBag.TypeAr = TypeAr;
            return View(list);
        }
        public IActionResult RowDetail(int ID,string Type)
        {
            AgricultureDBContext DBContext = new AgricultureDBContext();
            var Row = DBContext.WebsiteTypeDetails.Where(a => a.ID == ID && (a.IsActive == true || a.IsActive == null)).ToList().FirstOrDefault();
            ViewBag.TypeAr = Type;
            ViewBag.WebsitetypeID = Row.WebsitetypeID;
            //string TypeAr = DBContext.Websitetypes.Where(a => a.ID == ID).ToList().FirstOrDefault()?.TypeAr;

            //ViewBag.TypeAr = TypeAr;
            return View(Row);
        }
    }
}
