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
            public IActionResult Index(long CountryName = 0, long ItemName = 0)
            {
                AgricultureDBContext dbContext = new AgricultureDBContext();

            ////////////////////////list of Country////////////////////////////////
            var DataCountry = (from  c in dbContext.Countries
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
                                //join c in db.Countries on im.Country_Id equals c.ID

                            select new ItemVM
                            {
                                ID = i.ID,
                                Name_Ar = i.Name_Ar,
                                Name_En = i.Name_En,

                                //Country_Id = im.Country_Id,
                                //Item_ShortName_ID = im.Item_ShortName_ID,
                            })
                                 .Where(i => i.Name_Ar != null)

     .Distinct().OrderBy(i => i.Name_Ar).ToList();
            ViewData["ItemList"] = new SelectList(DataItem, "ID", "Name_Ar");

               if (CountryName > 0 && ItemName > 0)
               {
                //        var List = (from Exc in dbContext.Countries
                //                    join intext in dbContext.Im_Constrain_Initiator_Texts on Exc.ID equals intext.Im_Initiator_ID
                //                    where Exc.ID == CountryName && Im_In.Item_ShortName.Item.ID == ItemName && intext.IsActive == true
                //                    select new Im_InitiatorVM
                //                    {
                //                        IDInitiator = Im_In.ID,
                //                        InitiatorNameAr = Im_In.Country.Ar_Name,
                //                        InitiatorNameEn = Im_In.Country.En_Name,
                //                        ItemName = Im_In.Item_ShortName.Item.Name_Ar,

                //                        Country_Id = Im_In.Country_Id,
                //                        Item_ShortName_ID = Im_In.Item_ShortName_ID,
                //                        Item_ID = Im_In.Item_ShortName.Item_ID,
                //                        ShortName_Ar = Im_In.Item_ShortName.ShortName_Ar,
                //                        ConstrainText_Ar = intext.ConstrainText.ConstrainText_Ar,
                //                        ConstrainText_En = intext.ConstrainText.ConstrainText_En,
                //                        InSide_Certificate_Ar = intext.ConstrainText.InSide_Certificate_Ar,
                //                        InSide_Certificate_En = intext.ConstrainText.InSide_Certificate_En,

                //                    }).Distinct().Take(10).ToList();

                // return View(List);
                ////////////////////////////////////code from eslam ////////////////////////////////////////
                var dataConstrains_Country = (from Ecc in dbContext.Ex_CountryConstrains
                                              join c in dbContext.Countries on Ecc.Import_Country_ID equals c.ID
                                              where Ecc.User_Deletion_Id == null
                                              && Ecc.IsActive == true
                                              select new Ex_CountryConstrainVM
                                              {
                                                  ID = Ecc.ID,
                                                  IsStationAccreditation = (bool)Ecc.IsStationAccreditation,
                                                  IsCompanyAccreditation = (bool)Ecc.IsCompanyAccreditation,
                                                  IsFarmAccreditation = (bool)Ecc.IsFarmAccreditation,
                                                  ItemCategories_ID = Ecc.ItemCategories_ID,
                                                  //Import_Country_Name = c.Ar_Name,

                                                  //Item_Short_Name = Ecc.Item_ShortName.ShortName_Ar,
                                                  Item_ShortName_id = Ecc.Item_ShortName.ID
                                              }).ToList();

                //////////////////////////////////////////////////////////////////////////
                return View();
               }
             else
                {
                   return View();
               }
            }
        }
    }

