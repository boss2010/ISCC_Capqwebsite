using EF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            AgricultureDBContext dbContext=new AgricultureDBContext();

            
            return View(dbContext.WebsiteTypeDetails.ToList());
        }
        AgricultureDBContext db = new AgricultureDBContext();

        public IActionResult dash()
        {



           int People = db.People.ToList().Count();
            ViewBag.People = People;
           
           int Public_Organizations = db.Public_Organizations.ToList().Count();
            ViewBag.Public_Organizations = Public_Organizations;

            int Company_Nationals = db.Company_Nationals.ToList().Count();
            ViewBag.Company_Nationals = Company_Nationals;
            /////////////////////////////////////////////////////////////////////
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var Countries = (from PR in db.Im_CheckRequests.Where(a => a.IsAccepted == true)                         
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

                         }).ToList().Take(5);

            var Products = (from PR in db.Im_CheckRequests.Where(a => a.IsAccepted == true
                         //&& System.Data.Entity. DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Day == DateTime.Now.Day
                         //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Month == DateTime.Now.Month
                         //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Year == DateTime.Now.Year
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

                         }).ToList().Take(4);


            //الصادر
            var ProductEX = (from ex in db.Ex_CheckRequests.Where(a => a.IsAccepted == true
                         //&& System.Data.Entity. DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Day == DateTime.Now.Day
                         //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Month == DateTime.Now.Month
                         //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Year == DateTime.Now.Year
                         )
                            join It in db.Ex_CheckRequest_Items on ex.ID equals It.Ex_CheckRequest_ID

                            join In in db.Item_ShortNames on It.Item_ShortName_ID equals In.ID
                            group It by In.ShortName_Ar into g
                            orderby Math.Round((double)g.Sum(It => It.GrossWeight)) descending
                            select new ProductsEXVM
                            {
                                Country = g.Key,
                                CountOrders = Math.Round((double)g.Sum(It => It.GrossWeight / 1000)),

                            }).ToList().Take(4);

            var CountriesEx = (from ex in db.Ex_CheckRequests.Where(a => a.IsAccepted == true)
                             join It in db.Ex_CheckRequest_Items on ex.ID equals It.Ex_CheckRequest_ID
                             join In in db.Ex_CheckRequest_Data on ex.ID equals In.Ex_CheckRequest_ID
                             join IE in db.Countries on In.ExportCountry_Id equals IE.ID
                             group It by IE.Ar_Name into g
                             orderby Math.Round((double)g.Sum(It => It.GrossWeight)) descending
                             select new CountriesExVM
                             {
                                 Country = g.Key,
                                 CountOrders = Math.Round((double)g.Sum(It => It.GrossWeight) / 1000)

                             }).ToList().Take(5);
            //return View();
            var vm = new DashboardVM
            {
                Countries = Countries,
                CountriesEx = CountriesEx,
                Products = Products,
                ProductsEX = ProductEX,

            };

            return View(vm);

        }
        //////////////////////////////////////////////////////////////
        //المنتجات


    }
}
