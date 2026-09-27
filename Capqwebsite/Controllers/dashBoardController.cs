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
            return View(db.WebsiteTypeDetails.ToList());
        }
        AgricultureDBContext db = new AgricultureDBContext();
		public IActionResult Dash(
			DateTime? fromDate,
			DateTime? toDate,
			int? year,
			int? month,
			int? productId,
			string direction = "all")
		{
			// =========================================================
			// 1) السنة المالية
			// =========================================================

			int yearNo = year ?? (
				DateTime.Now.Month >= 4
					? DateTime.Now.Year
					: DateTime.Now.Year - 1
			);


			// =========================================================
			// 2) تحديد الفترة
			// السنة المالية من 1 أبريل إلى 31 مارس
			// =========================================================

			DateTime startDate;
			DateTime endDate;

			if (fromDate.HasValue)
			{
				startDate = fromDate.Value.Date;
			}
			else
			{
				startDate = new DateTime(yearNo, 4, 1);
			}


			if (toDate.HasValue)
			{
				// اليوم المختار بالكامل
				endDate = toDate.Value.Date.AddDays(1);
			}
			else
			{
				// نهاية السنة المالية
				endDate = new DateTime(yearNo + 1, 4, 1);
			}


			// =========================================================
			// 3) البيانات العامة
			// =========================================================

			ViewBag.People = db.People.Count();

			ViewBag.Public_Organizations =
				db.Public_Organizations.Count();

			ViewBag.Company_Nationals =
				db.Company_Nationals.Count();


			// =========================================================
			// 4) IMPORT REQUESTS
			// =========================================================

			var importRequests = db.Im_CheckRequests
				.Where(x =>
					x.IsAccepted == true &&
					x.IsAccepted_Date.HasValue &&
					x.IsAccepted_Date.Value >= startDate &&
					x.IsAccepted_Date.Value < endDate
				);


			// فلتر الشهر
			if (month.HasValue)
			{
				importRequests = importRequests.Where(x =>
					x.IsAccepted_Date.Value.Month == month.Value
				);
			}


			// =========================================================
			// 5) EXPORT REQUESTS
			// =========================================================

			var exportRequests = db.Ex_CheckRequests
				.Where(x =>
					x.IsAccepted == true &&
					x.User_Creation_Date.HasValue &&
					x.User_Creation_Date.Value >= startDate &&
					x.User_Creation_Date.Value < endDate
				);


			// فلتر الشهر
			if (month.HasValue)
			{
				exportRequests = exportRequests.Where(x =>
					x.User_Creation_Date.Value.Month == month.Value
				);
			}


			// =========================================================
			// 6) عدد أذون الاستيراد
			// =========================================================

			int importOrders = importRequests.Count();


			// =========================================================
			// 7) عدد أذون التصدير
			// =========================================================

			int exportOrders = exportRequests.Count();


			// =========================================================
			// 8) IMPORT ITEMS
			// =========================================================

			var importItems =
				from PR in importRequests

				join Im in db.Im_CheckRequset_Shipping_Methods
					on PR.ID equals Im.Im_CheckRequest_ID

				join It in db.Im_CheckRequest_Items
					on Im.ID equals It.Im_CheckRequset_Shipping_Method_ID

				join In in db.Item_ShortNames
					on It.Item_ShortName_ID equals In.ID

				select new
				{
					Request = PR,

					Item = It,

					ProductId = In.ID,

					ProductName = In.ShortName_Ar
				};


			// =========================================================
			// 9) EXPORT ITEMS
			// =========================================================

			var exportItems =
				from EX in exportRequests

				join It in db.Ex_CheckRequest_Items
					on EX.ID equals It.Ex_CheckRequest_ID

				join In in db.Item_ShortNames
					on It.Item_ShortName_ID equals In.ID

				select new
				{
					Request = EX,

					Item = It,

					ProductId = In.ID,

					ProductName = In.ShortName_Ar
				};


			// =========================================================
			// 10) فلتر المنتج
			// =========================================================

			if (productId.HasValue)
			{
				importItems =
					importItems.Where(x =>
						x.ProductId == productId.Value);

				exportItems =
					exportItems.Where(x =>
						x.ProductId == productId.Value);
			}


			// =========================================================
			// 11) فلتر الاتجاه
			// =========================================================

			if (direction == "import")
			{
				exportItems =
					exportItems.Where(x => false);

				exportRequests =
					exportRequests.Where(x => false);

				exportOrders = 0;
			}


			if (direction == "export")
			{
				importItems =
					importItems.Where(x => false);

				importRequests =
					importRequests.Where(x => false);

				importOrders = 0;
			}


			// =========================================================
			// 12) أعلى منتجات الاستيراد
			// =========================================================

			var Products = importItems
				.GroupBy(x => x.ProductName)

				.Select(g => new ProductsVM
				{
					Country = g.Key,

					CountOrders =
						Math.Round(
							g.Sum(x =>
								(double)x.Item.GrossWeight
							) / 1000
						)
				})

				.OrderByDescending(x => x.CountOrders)

				.Take(5)

				.ToList();


			// =========================================================
			// 13) أعلى منتجات التصدير
			// =========================================================

			var ProductsEX = exportItems
				.GroupBy(x => x.ProductName)

				.Select(g => new ProductsEXVM
				{
					Country = g.Key,

					CountOrders =
						Math.Round(
							g.Sum(x =>
								(double)x.Item.GrossWeight
							) / 1000
						)
				})

				.OrderByDescending(x => x.CountOrders)

				.Take(5)

				.ToList();


			// =========================================================
			// 14) أعلى دول الاستيراد
			// =========================================================

			var CountriesQuery =
				from X in importItems

				join In in db.Im_CheckRequest_Data
					on X.Request.ID equals In.Im_CheckRequest_ID

				join IE in db.Countries
					on In.ExportCountry_Id equals IE.ID

				group X by IE.Ar_Name into g

				select new CountriesVM
				{
					Country = g.Key,

					CountOrders =
						Math.Round(
							g.Sum(x =>
								(double)x.Item.GrossWeight
							) / 1000
						)
				};


			var CountriesList = CountriesQuery
				.OrderByDescending(x => x.CountOrders)
				.Take(5)
				.ToList();


			// =========================================================
			// 15) أعلى دول التصدير
			// =========================================================
			//
			// ملاحظة:
			// لو جدول Ex_CheckRequest_Data عندك مختلف
			// عدّل الجزء الخاص به فقط.
			// =========================================================

			var CountriesExQuery =
				from X in exportItems

				join In in db.Ex_CheckRequest_Data
					on X.Request.ID equals In.Ex_CheckRequest_ID

				join IE in db.Countries
					on In.ExportCountry_Id equals IE.ID

				group X by IE.Ar_Name into g

				select new CountriesExVM
				{
					Country = g.Key,

					CountOrders =
						Math.Round(
							g.Sum(x =>
								(double)x.Item.GrossWeight
							) / 1000
						)
				};


			var CountriesExList = CountriesExQuery
				.OrderByDescending(x => x.CountOrders)
				.Take(5)
				.ToList();


			// =========================================================
			// 16) إجمالي كمية الاستيراد
			// =========================================================

			double totalImport =
				importItems.Sum(x =>
					(double?)x.Item.GrossWeight
				) ?? 0;


			// =========================================================
			// 17) إجمالي كمية التصدير
			// =========================================================

			double totalExport =
				exportItems.Sum(x =>
					(double?)x.Item.GrossWeight
				) ?? 0;


			// =========================================================
			// 18) تحويل KG إلى TON
			// =========================================================

			totalImport = totalImport / 1000;

			totalExport = totalExport / 1000;


			// =========================================================
			// 19) ViewBag الخاصة بالفلاتر
			// =========================================================

			ViewBag.year = yearNo;

			ViewBag.Year = yearNo;

			ViewBag.Month = month;

			ViewBag.ProductId = productId;

			ViewBag.Direction = direction;


			ViewBag.FromDate =
				fromDate.HasValue
					? fromDate.Value.ToString("yyyy-MM-dd")
					: startDate.ToString("yyyy-MM-dd");


			ViewBag.ToDate =
				toDate.HasValue
					? toDate.Value.ToString("yyyy-MM-dd")
					: endDate
						.AddDays(-1)
						.ToString("yyyy-MM-dd");


			// =========================================================
			// 20) قائمة المنتجات
			// =========================================================

			ViewBag.ProductsList =
				db.Item_ShortNames
					.OrderBy(x => x.ShortName_Ar)
					.ToList();


			// =========================================================
			// 21) Dashboard ViewModel
			// =========================================================

			var vm = new DashboardVM
			{
				Countries = CountriesList,

				CountriesEx = CountriesExList,

				Products = Products,

				ProductsEX = ProductsEX,

				TotalImportTons =
					Math.Round(totalImport, 2),

				TotalExportTons =
					Math.Round(totalExport, 2),

				ImportOrders =
					importOrders,

				ExportOrders =
					exportOrders
			};


			// =========================================================
			// 22) ViewBag إضافية للـDashboard
			// =========================================================

			ViewBag.TotalImportTons =
				Math.Round(totalImport, 2);

			ViewBag.TotalExportTons =
				Math.Round(totalExport, 2);

			ViewBag.TotalTons =
				Math.Round(
					totalImport + totalExport,
					2
				);

			ViewBag.ImportOrders =
				importOrders;

			ViewBag.ExportOrders =
				exportOrders;


			// =========================================================
			// 23) Return View
			// =========================================================

			return View(vm);
		}





		//    public IActionResult dash()
		//    {
		//        int yearNo = DateTime.Now.Month > 3
		//? DateTime.Now.Year
		//: DateTime.Now.Year - 1;
		//        int People = db.People.ToList().Count();
		//        ViewBag.People = People;

		//        int Public_Organizations = db.Public_Organizations.ToList().Count();
		//        ViewBag.Public_Organizations = Public_Organizations;

		//        int Company_Nationals = db.Company_Nationals.ToList().Count();
		//        ViewBag.Company_Nationals = Company_Nationals;
		//        /////////////////////////////////////////////////////////////////////
		//        var today = DateTime.Today;
		//        var tomorrow = today.AddDays(1);
		//        var Countries = (from PR in db.Im_CheckRequests.Where(a => a.IsAccepted == true && a.IsAccepted_Date.Value.Year == yearNo)
		//                         join Im in db.Im_CheckRequset_Shipping_Methods on PR.ID equals Im.Im_CheckRequest_ID
		//                         join It in db.Im_CheckRequest_Items on Im.ID equals It.Im_CheckRequset_Shipping_Method_ID
		//                         join In in db.Im_CheckRequest_Data on PR.ID equals In.Im_CheckRequest_ID
		//                         join IE in db.Countries on In.ExportCountry_Id equals IE.ID
		//                         group It by IE.Ar_Name into g
		//                         orderby Math.Round((double)g.Sum(It => It.GrossWeight)) descending
		//                         select new CountriesVM
		//                         {
		//                             Country = g.Key,
		//                             CountOrders = Math.Round((double)g.Sum(It => It.GrossWeight) / 1000)

		//                         }).Take(5).ToList();

		//        var Products = (from PR in db.Im_CheckRequests.Where(a => a.IsAccepted == true
		//                     //&& System.Data.Entity. DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Day == DateTime.Now.Day
		//                     //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Month == DateTime.Now.Month
		//                     //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Year == DateTime.Now.Year
		//                     //&& a.IsAccepted_Date.Value.Year == 2025
		//                     && a.IsAccepted_Date.Value.Year == yearNo
		//                     )
		//                        join Im in db.Im_CheckRequset_Shipping_Methods on PR.ID equals Im.Im_CheckRequest_ID
		//                        join It in db.Im_CheckRequest_Items on Im.ID equals It.Im_CheckRequset_Shipping_Method_ID

		//                        join In in db.Item_ShortNames on It.Item_ShortName_ID equals In.ID
		//                        group It by In.ShortName_Ar into g
		//                        orderby Math.Round((double)g.Sum(It => It.GrossWeight)) descending
		//                        select new ProductsVM
		//                        {
		//                            Country = g.Key,
		//                            CountOrders = Math.Round((double)g.Sum(It => It.GrossWeight / 1000)),

		//                        }).Take(4).ToList();


		//        //الصادر
		//        var ProductEX = (from ex in db.Ex_CheckRequests.Where(a => a.IsAccepted == true
		//                     //&& System.Data.Entity. DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Day == DateTime.Now.Day
		//                     //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Month == DateTime.Now.Month
		//                     //&& System.Data.Entity.DbFunctions.TruncateTime(a.IsAccepted_Date).Value.Year == DateTime.Now.Year
		//                     && a.User_Creation_Date.Value.Year == yearNo
		//                     )
		//                         join It in db.Ex_CheckRequest_Items on ex.ID equals It.Ex_CheckRequest_ID

		//                         join In in db.Item_ShortNames on It.Item_ShortName_ID equals In.ID
		//                         group It by In.ShortName_Ar into g
		//                         orderby Math.Round((double)g.Sum(It => It.GrossWeight)) descending
		//                         select new ProductsEXVM
		//                         {
		//                             Country = g.Key,
		//                             CountOrders = Math.Round((double)g.Sum(It => It.GrossWeight / 1000)),

		//                         }).Take(4).ToList();

		//        var CountriesEx = (from ex in db.Ex_CheckRequests.Where(a => a.IsAccepted == true && a.User_Creation_Date.Value.Year == yearNo)
		//                               //&& a.IsAccepted_Date.Value.Year == 2025)
		//                           join It in db.Ex_CheckRequest_Items on ex.ID equals It.Ex_CheckRequest_ID
		//                           join In in db.Ex_CheckRequest_Data on ex.ID equals In.Ex_CheckRequest_ID
		//                           join IE in db.Countries on In.ExportCountry_Id equals IE.ID
		//                           group It by IE.Ar_Name into g
		//                           orderby Math.Round((double)g.Sum(It => It.GrossWeight)) descending
		//                           select new CountriesExVM
		//                           {
		//                               Country = g.Key,
		//                               CountOrders = Math.Round((double)g.Sum(It => It.GrossWeight) / 1000)

		//                           }).Take(5).ToList();
		//        //return View();
		//        var vm = new DashboardVM
		//        {
		//            Countries = Countries,
		//            CountriesEx = CountriesEx,
		//            Products = Products,
		//            ProductsEX = ProductEX,

		//        };
		//        ViewBag.year = yearNo;
		//        return View(vm);



		//    }
	}
}