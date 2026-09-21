using EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ViewModels;

namespace Capqwebsite.Controllers
{
	public class ExportingProcedureController : Controller
	{
	


		// =========================================================
		// الصفحة الرئيسية
		// =========================================================

		[AllowAnonymous]
		[Route("/ExportingProcedure/Index")]
		public IActionResult Index(long CountryID = 0, long ItemID = 0)
		{
			AgricultureDBContext _dbContext = new AgricultureDBContext();
			// -----------------------------------------------------
			// الدول التي لديها اشتراطات فقط
			// -----------------------------------------------------

			var DataCountry = (
				from c in _dbContext.Countries

				join ecc in _dbContext.Ex_CountryConstrains
					on c.ID equals ecc.Import_Country_ID

				where c.IsActive == true
					  && c.User_Deletion_Date == null
					  && c.User_Deletion_Id == null
					  && ecc.IsActive == true
					  && ecc.User_Deletion_Id == null

				select new Ex_CountryConstrainVM
				{
					IDCountry = c.ID,
					CountryNameAr = c.Ar_Name,
					CountryNameEn = c.En_Name
				}
			)
			.Distinct()
			.OrderBy(x => x.CountryNameAr)
			.ToList();



			ViewData["CountryList"] = new SelectList(
				DataCountry,
				"IDCountry",
				"CountryNameAr",
				CountryID
			);


			// -----------------------------------------------------
			// النباتات الخاصة بالدولة المختارة
			// -----------------------------------------------------

			var DataItem = new List<ItemVM>();

			if (CountryID > 0)
			{
				DataItem = (
					from i in _dbContext.Item_ShortNames

					join ecc in _dbContext.Ex_CountryConstrains
						on i.ID equals ecc.Item_ShortName_id

					where ecc.Import_Country_ID == CountryID
						  && ecc.IsActive == true
						  && ecc.User_Deletion_Id == null
						  && i.ShortName_Ar != null

					select new ItemVM
					{
						ID = i.ID,
						Name_Ar = i.ShortName_Ar,
						Name_En = i.ShortName_En
					}
				)
				.Distinct()
				.OrderBy(x => x.Name_Ar)
				.ToList();
			}


			ViewData["ItemList"] = new SelectList(
				DataItem,
				"ID",
				"Name_Ar",
				ItemID
			);


			// -----------------------------------------------------
			// الاشتراطات
			// -----------------------------------------------------

			List<ExportingProcedureVM> dataConstrains_Country = new();


			if (CountryID > 0 && ItemID > 0)
			{
				dataConstrains_Country = GetConstraintsData(
					CountryID,
					ItemID
				);
			}


			return View(dataConstrains_Country);
		}



		// =========================================================
		// AJAX
		// جلب النباتات الخاصة بالدولة
		// =========================================================

		[HttpGet]
		[AllowAnonymous]
		[Route("/ExportingProcedure/GetItemsByCountry")]
		public IActionResult GetItemsByCountry(long CountryID)
		{
			AgricultureDBContext _dbContext = new AgricultureDBContext();

			if (CountryID <= 0)
			{
				return Json(new List<object>());
			}


			var DataItem = (
				from i in _dbContext.Item_ShortNames

				join ecc in _dbContext.Ex_CountryConstrains
					on i.ID equals ecc.Item_ShortName_id

				where ecc.Import_Country_ID == CountryID
					  && ecc.IsActive == true
					  && ecc.User_Deletion_Id == null
					  && i.ShortName_Ar != null

				select new
				{
					ID = i.ID,
					Name_Ar = i.ShortName_Ar,
					Name_En = i.ShortName_En
				}
			)
			.Distinct()
			.OrderBy(x => x.Name_Ar)
			.ToList();


			return Json(DataItem);
		}



		// =========================================================
		// AJAX
		// جلب الاشتراطات الخاصة بالدولة والنبات
		// =========================================================

		[HttpGet]
		[AllowAnonymous]
		[Route("/ExportingProcedure/GetConstraints")]
		public IActionResult GetConstraints(
			long CountryID,
			long ItemID)
		{
			if (CountryID <= 0 || ItemID <= 0)
			{
				return PartialView(
					"_PartialExporting",
					new List<ExportingProcedureVM>()
				);
			}


			// استدعاء الدالة الداخلية
			var dataConstrains_Country =
				GetConstraintsData(CountryID, ItemID);


			return PartialView(
				"_PartialExporting",
				dataConstrains_Country
			);
		}



		// =========================================================
		// Function لجلب الاشتراطات من قاعدة البيانات
		// =========================================================

		private List<ExportingProcedureVM> GetConstraintsData(
	long CountryID,
	long ItemID)
		{

			AgricultureDBContext _dbContext = new AgricultureDBContext();

			var result =
				(
					from ecc in _dbContext.Ex_CountryConstrains

					join c in _dbContext.Countries
						on ecc.Import_Country_ID equals c.ID

					join exCTxt in _dbContext.Ex_CountryConstrain_Texts
						on ecc.ID equals exCTxt.CountryConstrain_ID

					join exTxt in _dbContext.EX_Constrain_Texts
						on exCTxt.EX_Constrain_Text_ID equals exTxt.ID

					where ecc.Import_Country_ID == CountryID
						  && ecc.Item_ShortName_id == ItemID
						  && ecc.IsActive == true
						  && ecc.User_Deletion_Id == null

						  && exCTxt.IsActive == true
						  && exCTxt.User_Deletion_Id == null

						  && exTxt.IsActive == true
						  && exTxt.User_Deletion_Id == null

					select new ExportingProcedureVM
					{
						CountryName = c.Ar_Name,

						itemName =
							ecc.Item_ShortName.Item.Name_Ar,

						ShortName_Ar =
							ecc.Item_ShortName.ShortName_Ar,

						ConstrainText_Ar =
							exTxt.ConstrainText_Ar,

						ConstrainText_En =
							exTxt.ConstrainText_En,

						InSide_Certificate_Ar =
							exTxt.InSide_Certificate_Ar,

						Item_ID =
							ecc.Item_ShortName.Item.ID,

						InSide_Certificate_En =
							exTxt.InSide_Certificate_En
					}
				)
				.ToList();

			return result;
		}




	}
}

//using System.Globalization;
//using EF.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using ViewModels;


//namespace Capqwebsite.Controllers
//{
//    public class ExportingProcedureController : Controller
//    {

//        [AllowAnonymous]
//        [Route("/ExportingProcedure/Index")]
//        public IActionResult Index(long CountryID = 0, long ItemID = 0)
//        {
//            AgricultureDBContext dbContext = new AgricultureDBContext();

//            ////////////////////////list of Country////////////////////////////////
//            var DataCountry = (from c in dbContext.Countries
//                               where c.IsActive == true 
//                               && c.User_Deletion_Date == null
//                               && c.User_Deletion_Id == null
//                               select new Ex_CountryConstrainVM
//                               {
//                                   IDCountry = c.ID,
//                                   CountryNameAr = c.Ar_Name,
//                                   CountryNameEn = c.En_Name,
//                                   //Country_Id = im.Country_Id,
//                                   //Item_ShortName_ID = im.Item_ShortName_ID,
//                               }).Distinct().OrderBy(c => c.CountryNameAr).ToList();


//            ViewData["CountryList"] = new SelectList(DataCountry, "IDCountry", "CountryNameAr");

//            //////////////////////list of Im_Initiators////////////////////////////////


//            var DataItem = (from i in dbContext.Item_ShortNames
//                            join ecc in dbContext.Ex_CountryConstrains
//                            on i.ID equals ecc.Item_ShortName_id
//                            where i.ShortName_Ar != null
//                            select new { i.ID, i.ShortName_Ar, i.ShortName_En })
//                            .GroupBy(x => x.ShortName_Ar)
//                            .Select(g => new ItemVM
//                            {
//                                ID = g.Min(x => x.ID),
//                                Name_Ar = g.Key,
//                                Name_En = g.Min(x => x.ShortName_En),
//                            })
//                            .OrderBy(x => x.Name_Ar).ToList();
//            ViewData["ItemList"] = new SelectList(DataItem, "ID", "Name_Ar");

//            if (CountryID > 0 && ItemID > 0)
//            {
//                var selectedShortName = dbContext.Item_ShortNames.Where(x => x.ID == ItemID).Select(x => x.ShortName_Ar).FirstOrDefault();

//                var dataConstrains_Country =
//                      (from ecc in dbContext.Ex_CountryConstrains
//                       join c in dbContext.Countries
//                           on ecc.Import_Country_ID equals c.ID
//                       join exCTxt in dbContext.Ex_CountryConstrain_Texts
//                           on ecc.ID equals exCTxt.CountryConstrain_ID
//                       join exTxt in dbContext.EX_Constrain_Texts
//                           on exCTxt.ID equals exTxt.EX_Constrain_Country_Item_ID
//                       where ecc.Import_Country_ID == CountryID
//                       && ecc.Item_ShortName.ShortName_Ar == selectedShortName
//                       && ecc.IsActive == true
//                       && ecc.User_Deletion_Id == null
//                       select new ExportingProcedureVM
//                       {
//                           CountryName = c.Ar_Name,
//                           itemName = ecc.Item_ShortName.Item.Name_Ar,
//                           ShortName_Ar = ecc.Item_ShortName.ShortName_Ar,
//                           ConstrainText_Ar = exTxt.ConstrainText_Ar,
//                           ConstrainText_En = exTxt.ConstrainText_En,
//                           InSide_Certificate_Ar = exTxt.InSide_Certificate_Ar,
//                           Item_ID = ecc.Item_ShortName.Item.ID,
//                           InSide_Certificate_En = exTxt.InSide_Certificate_En
//                       }).ToList();
//                return View(dataConstrains_Country);

//            }
//            else
//            {
//                return View();
//            }
//        }


//    }
//}

