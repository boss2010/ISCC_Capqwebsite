using System.Globalization;
using EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ViewModels;

namespace Capqwebsite.Controllers
{
    public class ImportingProcedureController : Controller
    {
        [AllowAnonymous]
        [Route("/ImportingProcedure/Index")]
        public IActionResult Index(long ImInitiatorID = 0, long ItemID = 0)
        {
            AgricultureDBContext dbContext = new AgricultureDBContext();

            //////////////////////list of Im_Initiators////////////////////////////////
            var DataIm_Initiator = (from im in dbContext.Im_Initiators
                                    join c in dbContext.Countries on im.Country_Id equals c.ID
                                    where im.IsActive == true
                                    select new Im_InitiatorVM
                                    {
                                        IDInitiator = c.ID,
                                        InitiatorNameAr = c.Ar_Name,
                                        InitiatorNameEn = c.En_Name,
                                        //Country_Id = im.Country_Id,
                                        //Item_ShortName_ID = im.Item_ShortName_ID,
                                    }).Distinct().OrderBy(x => x.InitiatorNameAr).ToList();


            ViewData["ImInitiatorList"] = new SelectList(DataIm_Initiator, "IDInitiator", "InitiatorNameAr");
            var culture = new CultureInfo("ar-SA");
            //////////////////////list of Im_Initiators////////////////////////////////
           
            var DataItem = (from i in dbContext.Items
                            join Im_In in dbContext.Im_Initiators
                            on i.ID equals Im_In.Item_ShortName.Item.ID

                            select new ItemVM
                            {
                                ID = Im_In.Item_ShortName.Item.ID,
                                Name_Ar = i.Name_Ar,
                                Name_En = i.Name_En,

                                //Country_Id = im.Country_Id,
                                //Item_ShortName_ID = im.Item_ShortName_ID,
                            })
                                .Where(i => i.Name_Ar != null)

    .Distinct().OrderBy(i => i.Name_Ar).ToList();
            ViewData["ItemList"] = new SelectList(DataItem, "ID", "Name_Ar");
            ////////////////////////////

            if (ImInitiatorID > 0 && ItemID > 0)
            {
                var List = (from Im_In in dbContext.Im_Initiators
                            join intext in dbContext.Im_Constrain_Initiator_Texts on Im_In.ID equals intext.Im_Initiator_ID
                            where Im_In.Country_Id == ImInitiatorID && Im_In.Item_ShortName.Item.ID == ItemID && intext.IsActive==true
                            select new Im_InitiatorVM
                            {
                                IDInitiator = Im_In.ID,
                                InitiatorNameAr = Im_In.Country.Ar_Name,
                                InitiatorNameEn = Im_In.Country.En_Name,
                                ItemName = Im_In.Item_ShortName.Item.Name_Ar,

                                Country_Id = Im_In.Country_Id,
                                Item_ShortName_ID = Im_In.Item_ShortName_ID,
                                Item_ID = Im_In.Item_ShortName.Item_ID,
                                ShortName_Ar = Im_In.Item_ShortName.ShortName_Ar,
                                ConstrainText_Ar= intext.ConstrainText.ConstrainText_Ar,
                                ConstrainText_En = intext.ConstrainText.ConstrainText_En,
                                InSide_Certificate_Ar = intext.ConstrainText.InSide_Certificate_Ar,
                                InSide_Certificate_En = intext.ConstrainText.InSide_Certificate_En,

                            }).Distinct().ToList();

                return View(List);
            }
            else
            {
                return View();
            }
        }
    }
}
