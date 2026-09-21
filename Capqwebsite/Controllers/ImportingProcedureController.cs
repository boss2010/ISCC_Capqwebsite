using EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ViewModels;

namespace Capqwebsite.Controllers
{
	public class ImportingProcedureController : Controller
	{
		// =========================================================
		// الصفحة الرئيسية
		// =========================================================

		[AllowAnonymous]
		[Route("/ImportingProcedure/Index")]
		public IActionResult Index(long ImInitiatorID = 0,long ItemID = 0)
		{
			AgricultureDBContext _dbContext = new AgricultureDBContext();

			// -----------------------------------------------------
			// المناشئ / الدول التي لديها اشتراطات فقط
			// -----------------------------------------------------

			var DataInitiator = (
				from im in _dbContext.Im_Initiators
				join c in _dbContext.Countries on im.Country_Id equals c.ID
				join intext in _dbContext.Im_Constrain_Initiator_Texts on im.ID equals intext.Im_Initiator_ID
				where im.IsActive == true
					  && im.User_Deletion_Date == null
					  && im.User_Deletion_Id == null

					  && c.IsActive == true
					  && c.User_Deletion_Date == null
					  && c.User_Deletion_Id == null
					   && intext.IsActive == true
					  && intext.User_Deletion_Date == null
					  && intext.User_Deletion_Id == null
				select new Im_InitiatorVM
				{
					IDInitiator = c.ID,
					InitiatorNameAr = c.Ar_Name,
					InitiatorNameEn = c.En_Name
				}
			).Distinct().OrderBy(x => x.InitiatorNameAr).ToList();


			ViewData["ImInitiatorList"] = new SelectList(
				DataInitiator,
				"IDInitiator",
				"InitiatorNameAr",
				ImInitiatorID
			);


			// -----------------------------------------------------
			// النباتات الخاصة بالمنشأ المختار
			// -----------------------------------------------------

			var DataItem = new List<ItemVM>();

			if (ImInitiatorID > 0)
			{
				DataItem = (
					from i in _dbContext.Item_ShortNames
					join im in _dbContext.Im_Initiators on i.ID equals im.Item_ShortName_ID
					join intext in _dbContext.Im_Constrain_Initiator_Texts on im.ID equals intext.Im_Initiator_ID
					where im.Country_Id == ImInitiatorID
						  && im.IsActive == true
						  && im.User_Deletion_Date == null
						  && im.User_Deletion_Id == null
						  && i.ShortName_Ar != null
						  && i.User_Deletion_Date == null
						  && i.User_Deletion_Id == null
					select new ItemVM
					{
						ID = i.ID,
						Name_Ar =i.Item.Name_Ar +"/"+  i.ShortName_Ar,
						Name_En = i.Item.Name_En + "/" + i.ShortName_En
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

			List<Im_InitiatorVM> dataConstraints = new();


			if (ImInitiatorID > 0 && ItemID > 0)
			{
				dataConstraints = GetConstraintsData(
					ImInitiatorID,
					ItemID
				);
			}


			return View(dataConstraints);
		}


		// =========================================================
		// AJAX
		// جلب النباتات الخاصة بالمنشأ
		// =========================================================

		[HttpGet]
		[AllowAnonymous]
		[Route("/ImportingProcedure/GetItemsByInitiator")]
		public IActionResult GetItemsByInitiator(long ImInitiatorID)
		{
			AgricultureDBContext _dbContext = new AgricultureDBContext();

			if (ImInitiatorID <= 0)
			{
				return Json(new List<object>());
			}


			var DataItem = (
				from i in _dbContext.Item_ShortNames
				join im in _dbContext.Im_Initiators on i.ID equals im.Item_ShortName_ID
				join intext in _dbContext.Im_Constrain_Initiator_Texts on im.ID equals intext.Im_Initiator_ID
				where im.Country_Id == ImInitiatorID
					  && im.IsActive == true
					  && im.User_Deletion_Date == null
					  && im.User_Deletion_Id == null
					  && i.ShortName_Ar != null
					  && i.User_Deletion_Date == null
					  && i.User_Deletion_Id == null
				select new ItemVM
				{
					ID = i.ID,
					Name_Ar = i.Item.Name_Ar + "/" + i.ShortName_Ar,
					Name_En = i.Item.Name_En + "/" + i.ShortName_En
				}
				)
				.Distinct()
				.OrderBy(x => x.Name_Ar)
				.ToList();


			return Json(DataItem);
		}


		// =========================================================
		// AJAX
		// جلب الاشتراطات الخاصة بالمنشأ والنبات
		// =========================================================

		[HttpGet]
		[AllowAnonymous]
		[Route("/ImportingProcedure/GetConstraints")]
		public IActionResult GetConstraints(
			long ImInitiatorID,
			long ItemID)
		{
			if (ImInitiatorID <= 0 || ItemID <= 0)
			{
				return PartialView(
					"_PartialImporting",
					new List<Im_InitiatorVM>()
				);
			}


			var dataConstraints =
				GetConstraintsData(
					ImInitiatorID,
					ItemID
				);


			return PartialView(
				"_PartialImporting",
				dataConstraints
			);
		}


		// =========================================================
		// Function
		// جلب الاشتراطات من قاعدة البيانات
		// =========================================================

		private List<Im_InitiatorVM> GetConstraintsData(long ImInitiatorID,long ItemID)
		{
			AgricultureDBContext _dbContext = new AgricultureDBContext();


			var result = (
				from im in _dbContext.Im_Initiators
				join c in _dbContext.Countries on im.Country_Id equals c.ID
				join intext in _dbContext.Im_Constrain_Initiator_Texts on im.ID equals intext.Im_Initiator_ID

				where im.Country_Id == ImInitiatorID

					  && im.Item_ShortName_ID == ItemID

					  && im.IsActive == true
					  && im.User_Deletion_Date == null
					  && im.User_Deletion_Id == null

					  && c.IsActive == true
					  && c.User_Deletion_Date == null
					  && c.User_Deletion_Id == null

					  && intext.IsActive == true
					  && intext.User_Deletion_Date == null
					  && intext.User_Deletion_Id == null

				select new Im_InitiatorVM
				{
					IDInitiator = im.ID,
					InitiatorNameAr =c.Ar_Name,
					InitiatorNameEn =c.En_Name,
					ItemName =im.Item_ShortName.Item.Name_Ar,

					Country_Id =im.Country_Id,

					Item_ShortName_ID =
						im.Item_ShortName_ID,

					Item_ID =
						im.Item_ShortName.Item_ID,

					ShortName_Ar =
						im.Item_ShortName.ShortName_Ar,

					ConstrainText_Ar =
						intext.ConstrainText.ConstrainText_Ar,

					ConstrainText_En =
						intext.ConstrainText.ConstrainText_En,

					InSide_Certificate_Ar =
						intext.ConstrainText.InSide_Certificate_Ar,

					InSide_Certificate_En =
						intext.ConstrainText.InSide_Certificate_En
				}
			)
			.Distinct()
			.ToList();


			return result;
		}


		// =========================================================
		// إجراء فتح منشأ
		// =========================================================

		[AllowAnonymous]
		[Route("/ImportingProcedure/OpenSource")]
		public IActionResult OpenSource()
		{
			return View();
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
//    public class ImportingProcedureController : Controller
//    {
//        [AllowAnonymous]
//        [Route("/ImportingProcedure/Index")]
//        public IActionResult Index(long ImInitiatorID = 0, long ItemID = 0,string ShortName_Ar="")
//        {
//            AgricultureDBContext dbContext = new AgricultureDBContext();

//            //////////////////////list of Im_Initiators////////////////////////////////
//            var DataIm_Initiator = (from im in dbContext.Im_Initiators
//                                    join c in dbContext.Countries on im.Country_Id equals c.ID
//                                    where im.IsActive == true&&c.IsActive == true 
//                                    && c.User_Deletion_Date == null
//                               && c.User_Deletion_Id == null
//                               && im.User_Deletion_Date == null
//                               && im.User_Deletion_Id == null
//                                    select new Im_InitiatorVM
//                                    {
//                                        IDInitiator = c.ID,
//                                        InitiatorNameAr = c.Ar_Name,
//                                        InitiatorNameEn = c.En_Name,
//                                        //Country_Id = im.Country_Id,
//                                        //Item_ShortName_ID = im.Item_ShortName_ID,
//                                    }).Distinct().OrderBy(x => x.InitiatorNameAr).ToList();


//            ViewData["ImInitiatorList"] = new SelectList(DataIm_Initiator, "IDInitiator", "InitiatorNameAr");
//            var culture = new CultureInfo("ar-SA");
//            //////////////////////list of Im_Initiators////////////////////////////////

//            var DataItem = (from i in dbContext.Item_ShortNames
//                                join Im_In in dbContext.Im_Initiators on i.ID equals Im_In.Item_ShortName_ID
//                            where  i.User_Deletion_Date == null && i.User_Deletion_Id == null
//                                  && i.ShortName_Ar != null
//                            select new { i.ID, i.ShortName_Ar })
//                            .GroupBy(x => x.ShortName_Ar)
//                            .Select(g => new ItemVM
//                            {
//                                ID = g.Min(x => x.ID),
//                                Name_Ar = g.Key,
//                            })
//                            .OrderBy(x => x.Name_Ar).ToList();
//            ViewData["ItemList"] = new SelectList(DataItem, "ID", "Name_Ar");
//            ////////////////////////////

//            if (ImInitiatorID > 0 && ItemID > 0)

//            {
//                var selectedShortName = dbContext.Item_ShortNames.Where(x => x.ID == ItemID).Select(x => x.ShortName_Ar).FirstOrDefault();

//                var List = (from Im_In in dbContext.Im_Initiators
//                            join intext in dbContext.Im_Constrain_Initiator_Texts on Im_In.ID equals intext.Im_Initiator_ID
//                            where Im_In.Country_Id == ImInitiatorID 
//                            && Im_In.Item_ShortName.ShortName_Ar == selectedShortName
//                            && intext.IsActive==true
//                            select new Im_InitiatorVM
//                            {
//                                IDInitiator = Im_In.ID,
//                                InitiatorNameAr = Im_In.Country.Ar_Name,
//                                InitiatorNameEn = Im_In.Country.En_Name,
//                                ItemName = Im_In.Item_ShortName.Item.Name_Ar,

//                                Country_Id = Im_In.Country_Id,
//                                Item_ShortName_ID = Im_In.Item_ShortName_ID,
//                                Item_ID = Im_In.Item_ShortName.Item_ID,
//                                ShortName_Ar = Im_In.Item_ShortName.ShortName_Ar,
//                                ConstrainText_Ar= intext.ConstrainText.ConstrainText_Ar,
//                                ConstrainText_En = intext.ConstrainText.ConstrainText_En,
//                                InSide_Certificate_Ar = intext.ConstrainText.InSide_Certificate_Ar,
//                                InSide_Certificate_En = intext.ConstrainText.InSide_Certificate_En,

//                            }).Distinct().ToList();

//                return View(List);
//            }
//            else
//            {
//                return View();
//            }
//        }
//        [AllowAnonymous]//اجراءات فتح منشأ
//        [Route("/ImportingProcedure/OpenSource")]
//        public IActionResult OpenSource()
//        {
//            return View();
//        }

//    }

//}
