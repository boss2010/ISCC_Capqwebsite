using EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using ViewModels;

namespace Capqwebsite.Controllers
{
    public class NewsController : Controller
    {
        [AllowAnonymous]
        [Route("/News/Index")]
        public IActionResult Index(int ID)
        {
            AgricultureDBContext DBContext = new AgricultureDBContext();
            var list = (from ne in DBContext.WebsiteTypeDetails

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

                        }).ToList();
            string TypeAr = DBContext.Websitetypes
                .Where(a => a.ID == ID).ToList().FirstOrDefault()?.TypeAr;

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
