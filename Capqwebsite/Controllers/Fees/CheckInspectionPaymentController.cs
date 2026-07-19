using EF.Models;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace Capqwebsite.Controllers.Fees
{
    public class CheckInspectionPaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Check(string National_ID)
        {
            if (
                string.IsNullOrWhiteSpace(National_ID) ||
                National_ID.Length != 14 ||
                !National_ID.All(char.IsDigit)
               )
            {
                ModelState.AddModelError(
                    "National_ID",
                    "الرقم القومي يجب أن يكون 14 رقم"
                );

                return View();
            }

            using var context = new AgricultureDBContext();

            //int bankPort = Request.Host.Port ?? 80;
            string url = Request.Host.Host;

            var checklist = context.Fees_Altahsils
                .Where(x =>
                    x.National_ID == National_ID &&
                    x.Code_Bank == "NF" &&
                    x.Account_Type == 139)
                .ToList();

            var result = new CheckResultVM
            {
                Details = new List<PaymentResultVM>()
            };

            foreach (var item in checklist)
            {
                string response =
                    ResponseAcquirerCode.Response_Inspection(
                        item.OrderNumber,
                        //bankPort,
                        url);

                if (response == "00")
                {
                    item.IsSuccess_Bank = true;
                    item.Code_Bank = response;

                    result.Details.Add(
                        new PaymentResultVM
                        {
                            ID = item.ID,
                            Amount_Total = item.Amount_Total,
                            date = item.date,
                        });
                }
            }

            context.SaveChanges();

            result.SuccessCount =
                result.Details.Count;

            TempData["result"] =
                Newtonsoft.Json.JsonConvert
                .SerializeObject(result);

            return RedirectToAction("CheckResultData");
        }
        public IActionResult CheckResultData()
        {
            if (TempData["result"] == null)
                return RedirectToAction("Check");

            var model =
                Newtonsoft.Json.JsonConvert
                .DeserializeObject<CheckResultVM>(
                    TempData["result"].ToString());

            return View(model);
        }
    }
}
