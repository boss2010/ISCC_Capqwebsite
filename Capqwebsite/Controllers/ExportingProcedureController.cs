using System.Globalization;
using EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ViewModels;


namespace Capqwebsite.Controllers
{
    public class ExportingProcedureController : Controller
    {

        [AllowAnonymous]
        [Route("/ExportingProcedure/Index")]
        public IActionResult Index(long CountryID = 0, long ItemID = 0)
        {
            AgricultureDBContext dbContext = new AgricultureDBContext();

            ////////////////////////list of Country////////////////////////////////
            var DataCountry = (from c in dbContext.Countries
                               where c.IsActive == true
                               select new Ex_CountryConstrainVM
                               {
                                   IDCountry = c.ID,
                                   CountryNameAr = c.Ar_Name,
                                   CountryNameEn = c.En_Name,
                                   //Country_Id = im.Country_Id,
                                   //Item_ShortName_ID = im.Item_ShortName_ID,
                               }).Distinct().OrderBy(c => c.CountryNameAr).ToList();


            ViewData["CountryList"] = new SelectList(DataCountry, "IDCountry", "CountryNameAr");

            //////////////////////list of Im_Initiators////////////////////////////////
            
           
            var DataItem = (from i in dbContext.Items
                                join ecc in dbContext.Ex_CountryConstrains
                                on i.ID equals ecc.Item_ShortName.Item.ID

                             select new ItemVM
                            {
                                ID = ecc.Item_ShortName.Item.ID,
                                Name_Ar = i.Name_Ar,
                                Name_En = i.Name_En,

                                //Country_Id = im.Country_Id,
                                //Item_ShortName_ID = im.Item_ShortName_ID,
                            })
                                 .Where(i => i.Name_Ar != null)

     .Distinct().OrderBy(i => i.Name_Ar).ToList();
            ViewData["ItemList"] = new SelectList(DataItem, "ID", "Name_Ar");

            if (CountryID > 0 && ItemID > 0)
            {



                var dataConstrains_Country =
      (from ecc in dbContext.Ex_CountryConstrains
       join c in dbContext.Countries
           on ecc.Import_Country_ID equals c.ID
       join exCTxt in dbContext.Ex_CountryConstrain_Texts
           on ecc.ID equals exCTxt.CountryConstrain_ID
       join exTxt in dbContext.EX_Constrain_Texts
           on exCTxt.ID equals exTxt.EX_Constrain_Country_Item_ID
       where ecc.Import_Country_ID == CountryID && ecc.Item_ShortName.Item.ID == ItemID && ecc.IsActive==true&& ecc.User_Deletion_Id==null
       select new ExportingProcedureVM
       {
           CountryName = c.Ar_Name,
           itemName = ecc.Item_ShortName.Item.Name_Ar,
           ShortName_Ar = ecc.Item_ShortName.ShortName_Ar,
           ConstrainText_Ar = exTxt.ConstrainText_Ar,
           ConstrainText_En = exTxt.ConstrainText_En,
           InSide_Certificate_Ar = exTxt.InSide_Certificate_Ar,
           Item_ID = ecc.Item_ShortName.Item.ID,
           InSide_Certificate_En = exTxt.InSide_Certificate_En



       }).ToList();
                return View(dataConstrains_Country);

            }
            else
            {
                return View();
            }
        }
    }
}

