using EF.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ViewModels;

namespace Capqwebsite.Controllers.Fees
{
    public class ResitPaymentController : Controller
    {
        public IActionResult Index(long ID, string Order_No)
        {
            try
            {

                AgricultureDBContext _context = new AgricultureDBContext();

                HttpClient client = new HttpClient();
                //int Bank_port = Request.Host.Port.Value;
                string Url = Request.Host.Host;
                string ResponseCode = ResponseAcquirerCode.Response_Inspection(Order_No/* Bank_port, Url*/);
                if (ResponseCode == "00")
                {
                    var dataupdate = _context.Fees_Altahsils.Where(a => a.ID == ID && a.OrderNumber == Order_No).FirstOrDefault();
                    if (dataupdate != null)
                    {
                        dataupdate.IsSuccess_Bank = true;
                        dataupdate.Code_Bank = ResponseCode;
                        _context.SaveChanges();
                    }


                    var data = _context.Fees_Altahsils
                            .Where(a => a.ID == ID && a.OrderNumber == Order_No)
                            .Select(x => new FeesAltahsilDTO
                            {
                                ID = x.ID,
                                OrderNumber = x.OrderNumber,
                                Amount_Total = x.Amount_Total,
                                User_Creation_Date = x.User_Creation_Date,
                                Date = x.date,
                                Office = x.office,
                                Customs_Certificate_Number = x.Customs_Certificate_Number,
                                National_ID = x.National_ID,
                                Tax_Registry = x.Tax_Registry,
                                Commercial_Register = x.Commercial_Register,
                                Ledger_Number = x.Ledger_Number,
                                Name = x.Name,
                                FarmName = x.FarmName,
                                Details = x.Fees_Altahsil_Detiles
                                    .Select(d => new FeesAltahsilDetileDTO
                                    {
                                        Amount = d.Amount,
                                        FeesTypeName = d.FeesType != null
                                            ? d.FeesType.Name_Ar
                                            : null
                                    })
                                    .ToList()
                            })
                            .FirstOrDefault();

                    TempData["Success"] = "تم الدفع بنجاح";

                    return View(data);

                }
                else
                {
                    var data = _context.Fees_Altahsils.Where(a => a.ID == ID && a.OrderNumber == Order_No).FirstOrDefault();
                    if (data != null)
                    {
                        data.IsSuccess_Bank = false;
                        data.Code_Bank = ResponseCode;
                        _context.SaveChanges();
                    }
                    TempData["Error"] = "حدث خطأ أثناء تحديث عملية الدفع";
                    return RedirectToAction("Index", "Fees");
                }

            }
            catch (Exception ex)
            {

                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();

                String ErrorMessage = "";



                ErrorMessage += "Source :" + ex.Source;
                ErrorMessage += "Data :" + ex.Data.ToString();

                ErrorMessage += "GetBaseException :" + ex.GetBaseException().ToString();
                ErrorMessage += "HelpLink :" + ex.HelpLink;
                ErrorMessage += "HResult :" + ex.HResult.ToString();
                ErrorMessage += "Message :" + ex.Message;
                ErrorMessage += "LineNumber :" + line.ToString();
                ErrorMessage += "TargetSite :" + ex.TargetSite.ToString();
                ErrorMessage += "InnerException :" + ex.InnerException;


                return Json(new
                {
                    Result = "Error",
                    Message = ex.Message
                });
            }
        }
    }
}
