using EF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ViewModels;

namespace Capqwebsite.Controllers.Fees
{
    public class FeesController : Controller
    {
		private readonly ILogger<FeesController> _logger;

		public FeesController(ILogger<FeesController> logger)
		{
			_logger = logger;
		}
		public IActionResult Index()
        {
            return View();
        }

        public IActionResult GeneralPayment()
        {
            AgricultureDBContext _context = new AgricultureDBContext();

            var model = new FeesAltahsilVM();

            var ids = new byte[] {20,21,22,23,24,25,28,29,30,31,32,33,34};

            model.Fees = _context.FeesTypes
                .Where(x => ids.Contains(x.ID))
                .Select(x => new FeeVM
                {
                    FeesType_ID = x.ID,
                    FeesName = x.Name_Ar
                })
                .ToList();

            return View(model);
        }

        public IActionResult InspectionPayment()
        {
            AgricultureDBContext _context = new AgricultureDBContext();

            var model = new FeesAltahsilVM();
            var ids = new byte[] { 26, 27, 35 };

            model.Fees = _context.FeesTypes
                .Where(x => ids.Contains(x.ID))
                .Select(x => new FeeVM
                {
                    FeesType_ID = x.ID,
                    FeesName = x.Name_Ar
                })
                .ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveGeneralPayment(FeesAltahsilVM model)
        {
            AgricultureDBContext _context = new AgricultureDBContext();

            // الرقم القومي
            if (string.IsNullOrWhiteSpace(model.National_ID))
            {
                ModelState.AddModelError(
                    nameof(model.National_ID),
                    "الرقم القومي مطلوب");
            }
            else if (
                model.National_ID.Length != 14 ||
                !model.National_ID.All(char.IsDigit))
            {
                ModelState.AddModelError(
                    nameof(model.National_ID),
                    "الرقم القومي يجب أن يكون 14 رقم");
            }

            // لازم يختار رسم واحد على الأقل
            if (
                model.Fees == null ||
                !model.Fees.Any(x => x.Selected))
            {
                ModelState.AddModelError(
                    "",
                    "اختر رسم واحد على الأقل");
            }

            // المبلغ مطلوب للرسم المختار فقط
            for (int i = 0; i < model.Fees.Count; i++)
            {
                if (
                    model.Fees[i].Selected &&
                    (
                        model.Fees[i].Amount == null ||
                        model.Fees[i].Amount <= 0
                    )
                )
                {
                    ModelState.AddModelError(
                        $"Fees[{i}].Amount",
                        "ادخل قيمة الرسم");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //save in database

            String Order_No = "7" + Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 9) + Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 3);

            Fees_Altahsil fe = new Fees_Altahsil
            {
                Amount_Total = model.Amount_Total,
                office = model.office,
                Name = model.Name,
                National_ID = model.National_ID,
                Customs_Certificate_Number = model.Customs_Certificate_Number,
                OrderNumber = Order_No,
                Account_Type = 138,
                Payment_Type_ID = 125,
                Commercial_Register = model.Commercial_Register,
                Tax_Registry = model.Tax_Registry,
                User_Creation_Date = DateTime.Now,
                FarmName=model.FarmName,
                date = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Fees_Altahsils.Add(fe);
            _context.SaveChanges(); 

            foreach (var item in model.Fees.Where(x => x.Amount != null))
            {
                Fees_Altahsil_Detile det = new Fees_Altahsil_Detile
                {
                    FeesType_ID = item.FeesType_ID,
                    Amount = item.Amount,
                    Fees_Altahsil_ID = fe.ID,
                    User_Creation_Date = DateTime.Now,
                };

                _context.Fees_Altahsil_Detiles.Add(det);
            }

            _context.SaveChanges();


            decimal amount = 0;
            HttpClient client = new HttpClient();

            int? port = Request.Host.Port;
            int? Bank_port = Request.Host.Port;
            ViewBag.port = port;
            ViewBag.ScriptBankPort = Bank_port;

            ///في حالة الدفع بالفيزا///



                //API insert Transaction and Transaction Details

              amount = model.Amount_Total;



                string Url = Request.Host.Host;
            
                Out_SessionDTO ss = new Out_SessionDTO();

                string DomainName = Method_Bank.DomainName(port);

                string host = Request.Host.Host;
                string Succesurl = "";
                string Cancelurl = "";
                if (host == "10.10.21.12")
                {
                    Succesurl = "http://10.10.21.12:8071/" + "Resit/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
                    Cancelurl = "http://10.10.21.12:8071/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";
            }
                else if (host == "41.33.237.90")
                {
                    Succesurl = "http://41.33.237.90:8071/" + "Resit/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
                Cancelurl = "http://41.33.237.90:8071/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";
            }
                else if (host == "www.site.capq.gov.eg")
                {
                    Succesurl = DomainName + "Resit/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
                Cancelurl = DomainName + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";

            }
                else
                {
                Succesurl = DomainName + "Resit/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
                Cancelurl = DomainName + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";

            }


                ss = Method_Bank.Create_Session(amount, Order_No, Cancelurl, Succesurl, Bank_port, host);
                ViewBag.Url = Url;

                ViewBag.Ses_Id = ss.Session_Id;
                ViewBag.Ord_Id = ss.Order_No;
                ViewBag.amount = ss.amount;
                return View();

                

         
        }
        [HttpPost]
        public async Task<IActionResult> SaveInspectionPayment(FeesAltahsilVM model)
        {
            try
            {
				AgricultureDBContext _context = new AgricultureDBContext();

				// الرقم القومي
				if (string.IsNullOrWhiteSpace(model.National_ID))
				{
					ModelState.AddModelError(
						nameof(model.National_ID),
						"الرقم القومي مطلوب");
				}
				else if (
					model.National_ID.Length != 14 ||
					!model.National_ID.All(char.IsDigit))
				{
					ModelState.AddModelError(
						nameof(model.National_ID),
						"الرقم القومي يجب أن يكون 14 رقم");
				}

				// لازم يختار رسم واحد على الأقل
				if (
					model.Fees == null ||
					!model.Fees.Any(x => x.Selected))
				{
					ModelState.AddModelError(
						"",
						"اختر رسم واحد على الأقل");
				}

				// المبلغ مطلوب للرسم المختار فقط
				for (int i = 0; i < model.Fees.Count; i++)
				{
					if (
						model.Fees[i].Selected &&
						(
							model.Fees[i].Amount == null ||
							model.Fees[i].Amount <= 0
						)
					)
					{
						ModelState.AddModelError(
							$"Fees[{i}].Amount",
							"ادخل قيمة الرسم");
					}
				}

				if (!ModelState.IsValid)
				{
					return View(model);
				}
				//save in database

				String Order_No = "8" + Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 9) + Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 3);

				Fees_Altahsil fe = new Fees_Altahsil
				{
					Amount_Total = model.Amount_Total,
					office = model.office,
					Name = model.Name,
					National_ID = model.National_ID,
					Customs_Certificate_Number = model.Customs_Certificate_Number,
					OrderNumber = Order_No,
					Account_Type = 139,
					Payment_Type_ID = 125,
					Commercial_Register = model.Commercial_Register,
					Tax_Registry = model.Tax_Registry,
					User_Creation_Date = DateTime.Now,
					FarmName = model.FarmName,
					date = DateOnly.FromDateTime(DateTime.Now)
				};

				_context.Fees_Altahsils.Add(fe);
				_context.SaveChanges();

				foreach (var item in model.Fees.Where(x => x.Amount != null))
				{
					Fees_Altahsil_Detile det = new Fees_Altahsil_Detile
					{
						FeesType_ID = item.FeesType_ID,
						Amount = item.Amount,
						Fees_Altahsil_ID = fe.ID,
						User_Creation_Date = DateTime.Now,
					};

					_context.Fees_Altahsil_Detiles.Add(det);
				}

				_context.SaveChanges();


				decimal amount = 0;
				HttpClient client = new HttpClient();

				int? port = Request.Host.Port;
				int? Bank_port = Request.Host.Port;
				ViewBag.port = port;
				ViewBag.ScriptBankPort = Bank_port;
				_logger.LogInformation("Port: {port}", port);
				_logger.LogInformation("Bank Port: {BankPort}", Bank_port);

				///في حالة الدفع بالفيزا///



				//API insert Transaction and Transaction Details

				amount = model.Amount_Total;



				string Url = Request.Host.Host;

				Out_SessionDTO ss = new Out_SessionDTO();

				string DomainName = Method_Bank.DomainName(port);

				string host = Request.Host.Host;

				_logger.LogInformation("Host: {host}", host);
				_logger.LogInformation("Session ID: {Session}", ss?.Session_Id);

				string Succesurl = "";
				string Cancelurl = "";
				if (host == "10.10.21.12")
				{
					Succesurl = "http://10.10.21.12:8071/" + "ResitPayment/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
					Cancelurl = "http://10.10.21.12:8071/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";
				}
				else if (host == "41.33.237.90")
				{
					Succesurl = "http://41.33.237.90:8071/" + "ResitPayment/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
					Cancelurl = "http://41.33.237.90:8071/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";
				}
				else if (host == "www.capq.gov.eg")
				{
					Succesurl = DomainName + "ResitPayment/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
					Cancelurl = DomainName + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";

				}
				else
				{
					Succesurl = DomainName + "ResitPayment/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
					Cancelurl = DomainName + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";

				}

				string logPath = Path.Combine(
				AppContext.BaseDirectory,
				"payment_log.txt"
			);

				System.IO.File.AppendAllText(
					logPath,
					$"Date: {DateTime.Now}\r\n" +
					$"Host: {host}\r\n" +
					$"Port: {Bank_port}\r\n" +
					$"Domain: {DomainName}\r\n\r\n"
				);

				ss = Method_Bank.Create_SessionFor_Inspection(amount, Order_No, Cancelurl, Succesurl, Bank_port, host);

				if (ss == null)
				{
					return Content("ss is null");
				}

				if (string.IsNullOrEmpty(ss.Session_Id))
				{
					return Content("Session_Id is empty");
				}
				ViewBag.Url = Url;

				ViewBag.Ses_Id = ss.Session_Id;
				ViewBag.Ord_Id = ss.Order_No;
				ViewBag.amount = ss.amount;
				return View();
			}
            catch (Exception ex)
            {

				var error = $@"
Message: {ex.Message}

Source: {ex.Source}

StackTrace:
{ex.StackTrace}

InnerException:
{ex.InnerException?.Message}

TargetSite:
{ex.TargetSite}

Data:
{string.Join(Environment.NewLine, ex.Data.Cast<System.Collections.DictionaryEntry>()
   .Select(x => $"{x.Key} = {x.Value}"))}
";

				Console.WriteLine(error);

				// لو عاوز تعرضه في الصفحة
				ViewBag.Error = error;

				return Content(error); // أو return View(model);
			}
           

        }
    }
}
