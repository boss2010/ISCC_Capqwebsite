using EF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Data.Entity;
using System.Data.Entity;
using ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Capqwebsite.Controllers
{
    public class dashBoardController : Controller
    {
        public IActionResult Index()
        {

            AgricultureDBContext dbContext = new AgricultureDBContext();


            return View(dbContext.WebsiteTypeDetails.ToList());
        }
        AgricultureDBContext db = new AgricultureDBContext();

        public IActionResult dash()
        {
            int yearNo = DateTime.Now.Month > 3
    ? DateTime.Now.Year
    : DateTime.Now.Year - 1;
            int People = db.People.ToList().Count();
            ViewBag.People = People;

            int Public_Organizations = db.Public_Organizations.ToList().Count();
            ViewBag.Public_Organizations = Public_Organizations;

            int Company_Nationals = db.Company_Nationals.ToList().Count();
            ViewBag.Company_Nationals = Company_Nationals;
            /////////////////////////////////////////////////////////////////////
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var Countries = (from PR in db.Im_CheckRequests.Where(a => a.IsAccepted == true && a.IsAccepted_Date.Value.Year == yearNo)
                             join Im in db.Im_CheckRequset_Shipping_Methods on PR.ID equals Im.Im_CheckRequest_ID
                             join It in db.Im_CheckRequest_Items on Im.ID equals It.Im_CheckRequset_Shipping_Method_ID
                             join In in db.Im_CheckRequest_Data on PR.ID equals In.Im_CheckRequest_ID
                             join IE in db.Countries on In.ExportCountry_Id equals IE.ID
                             group It by IE.Ar_Name into g
                             orderby Math.Round((double)g.Sum(It => It.GrossWeight)) descending
                             select new CountriesVM
                             {
                                 Country = g.Key,
                                 CountOrders = Math.Round((double)g.Sum(It => It.GrossWeight) / 1000)

                             }).Take(5).ToList();

            var Products = (from PR in db.Im_CheckRequests.Where(a => a.IsAccepted == true
                         //&& System.Data.Entity. DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Day == DateTime.Now.Day
                         //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Month == DateTime.Now.Month
                         //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Year == DateTime.Now.Year
                         //&& a.IsAccepted_Date.Value.Year == 2025
                         && a.IsAccepted_Date.Value.Year == yearNo
                         )
                            join Im in db.Im_CheckRequset_Shipping_Methods on PR.ID equals Im.Im_CheckRequest_ID
                            join It in db.Im_CheckRequest_Items on Im.ID equals It.Im_CheckRequset_Shipping_Method_ID

                            join In in db.Item_ShortNames on It.Item_ShortName_ID equals In.ID
                            group It by In.ShortName_Ar into g
                            orderby Math.Round((double)g.Sum(It => It.GrossWeight)) descending
                            select new ProductsVM
                            {
                                Country = g.Key,
                                CountOrders = Math.Round((double)g.Sum(It => It.GrossWeight / 1000)),

                            }).Take(4).ToList();


            //الصادر
            var ProductEX = (from ex in db.Ex_CheckRequests.Where(a => a.IsAccepted == true
                         //&& System.Data.Entity. DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Day == DateTime.Now.Day
                         //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Month == DateTime.Now.Month
                         //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Year == DateTime.Now.Year
                         && a.User_Creation_Date.Value.Year == yearNo
                         )
                             join It in db.Ex_CheckRequest_Items on ex.ID equals It.Ex_CheckRequest_ID

                             join In in db.Item_ShortNames on It.Item_ShortName_ID equals In.ID
                             group It by In.ShortName_Ar into g
                             orderby Math.Round((double)g.Sum(It => It.GrossWeight)) descending
                             select new ProductsEXVM
                             {
                                 Country = g.Key,
                                 CountOrders = Math.Round((double)g.Sum(It => It.GrossWeight / 1000)),

                             }).Take(4).ToList();

            var CountriesEx = (from ex in db.Ex_CheckRequests.Where(a => a.IsAccepted == true && a.User_Creation_Date.Value.Year == yearNo)
                                   //&& a.IsAccepted_Date.Value.Year == 2025)
                               join It in db.Ex_CheckRequest_Items on ex.ID equals It.Ex_CheckRequest_ID
                               join In in db.Ex_CheckRequest_Data on ex.ID equals In.Ex_CheckRequest_ID
                               join IE in db.Countries on In.ExportCountry_Id equals IE.ID
                               group It by IE.Ar_Name into g
                               orderby Math.Round((double)g.Sum(It => It.GrossWeight)) descending
                               select new CountriesExVM
                               {
                                   Country = g.Key,
                                   CountOrders = Math.Round((double)g.Sum(It => It.GrossWeight) / 1000)

                               }).Take(5).ToList();
            //return View();
            var vm = new DashboardVM
            {
                Countries = Countries,
                CountriesEx = CountriesEx,
                Products = Products,
                ProductsEX = ProductEX,

            };
            ViewBag.year = yearNo;
            return View(vm);

        }
        //////////////////////////////////////////////////////////////
        //المنتجات


    }
}
