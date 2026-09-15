using EF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ViewModels;
using Capqwebsite.Services;

namespace Capqwebsite.Controllers.Fees
{
    public class FeesController : Controller
    {
        private const byte MartyrFeeTypeId = 56;
        private const byte LegacyPrivateMartyrFeeTypeId = 34;
        private const byte DifferenceFeeTypeId = 81;
        private const decimal MartyrFeeMinimumAmount = 5m;
		private const string MechanizationFeeName = "رسوم ميكنة";
		private const decimal MechanizationFixedAmount = 2m;
		private const decimal MechanizationRate = 0.009m;
		private static readonly bool GovernmentPaymentsEnabled = true;
		private static readonly bool PrivatePaymentsEnabled = true;
		private readonly ILogger<FeesController> _logger;

		public FeesController(ILogger<FeesController> logger)
		{
			_logger = logger;
		}
		public IActionResult Index()
        {
            return GovernmentPaymentsEnabled
                ? RedirectToAction(nameof(GeneralPayment))
                : RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult GovernmentPayments(
            string? search,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            return SuccessfulPayments(
                138,
                "عمليات الدفع الحكومي الناجحة",
                nameof(GovernmentPayments),
                search,
                fromDate,
                toDate,
                page);
        }

        [HttpGet]
        public IActionResult PrivatePayments(
            string? search,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            return SuccessfulPayments(
                139,
                "عمليات الدفع الخاص الناجحة",
                nameof(PrivatePayments),
                search,
                fromDate,
                toDate,
                page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkPaymentUsed(
            long id,
            int accountType,
            string? search,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            if (HttpContext.Session.GetString("UserSession") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (accountType != 138 && accountType != 139)
            {
                return BadRequest();
            }

            if (HttpContext.Session.GetString("UserRole") != "Administrator")
            {
                return Forbid();
            }

            var usedByUserName =
                HttpContext.Session.GetString("LoginUserName") ?? "admin";

            using var context = new AgricultureDBContext();
            var usedDate = DateTime.Now;
            var updatedRows = context.Fees_Altahsils
                .Where(x =>
                    x.ID == id &&
                    x.Account_Type == accountType &&
                    x.IsSuccess_Bank == true &&
                    x.Code_Bank == "00" &&
                    !x.Is_Used)
                .ExecuteUpdate(setters => setters
                    .SetProperty(x => x.Is_Used, true)
                    .SetProperty(x => x.Used_Date, usedDate)
                    .SetProperty(x => x.Used_By_User_Name, usedByUserName));

            TempData[updatedRows == 1 ? "UsageSuccess" : "UsageInfo"] =
                updatedRows == 1
                    ? "تم تسجيل استخدام العملية بنجاح."
                    : "العملية مستخدمة بالفعل أو غير متاحة.";

            return RedirectToAction(
                accountType == 138
                    ? nameof(GovernmentPayments)
                    : nameof(PrivatePayments),
                new
                {
                    search,
                    fromDate = fromDate?.ToString("yyyy-MM-dd"),
                    toDate = toDate?.ToString("yyyy-MM-dd"),
                    page = Math.Max(1, page)
                });
        }

        [HttpGet]
        public IActionResult ExportGovernmentPayments(
            string? search,
            DateTime? fromDate,
            DateTime? toDate)
        {
            return ExportPayments(
                138,
                "عمليات الدفع الحكومي الناجحة",
                "GovernmentPayments",
                search,
                fromDate,
                toDate);
        }

        [HttpGet]
        public IActionResult ExportPrivatePayments(
            string? search,
            DateTime? fromDate,
            DateTime? toDate)
        {
            return ExportPayments(
                139,
                "عمليات الدفع الخاص الناجحة",
                "PrivatePayments",
                search,
                fromDate,
                toDate);
        }

        private IActionResult ExportPayments(
            int accountType,
            string reportTitle,
            string filePrefix,
            string? search,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (HttpContext.Session.GetString("UserSession") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            search = search?.Trim();
            using var context = new AgricultureDBContext();

            var query = context.Fees_Altahsils
                .AsNoTracking()
                .Where(x =>
                    x.Account_Type == accountType &&
                    x.IsSuccess_Bank == true &&
                    x.Code_Bank == "00");

            if (!string.IsNullOrWhiteSpace(search))
            {
                var hasNumericSearch = long.TryParse(
                    search,
                    out var numericSearch);

                query = query.Where(x =>
                    (hasNumericSearch && x.ID == numericSearch) ||
                    (x.OrderNumber != null && x.OrderNumber.Contains(search)) ||
                    x.National_ID.Contains(search) ||
                    (x.Name != null && x.Name.Contains(search)) ||
                    (x.office != null && x.office.Contains(search)) ||
                    (x.Department != null && x.Department.Contains(search)) ||
                    (x.Item != null && x.Item.Contains(search)) ||
                    (x.Ledger_Number != null &&
                     x.Ledger_Number.Contains(search)) ||
                    (x.Customs_Certificate_Number != null &&
                     x.Customs_Certificate_Number.Contains(search)));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x =>
                    x.User_Creation_Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date.AddDays(1);
                query = query.Where(x =>
                    x.User_Creation_Date < endDate);
            }

            var payments = query
                .OrderByDescending(x => x.User_Creation_Date)
                .ThenByDescending(x => x.ID)
                .Select(x => new SuccessfulPaymentVM
                {
                    ID = x.ID,
                    OrderNumber = x.OrderNumber,
                    CreationDate = x.User_Creation_Date,
                    PaymentDate = x.date,
                    TotalAmount = x.Amount_Total,
                    Office = x.office,
                    Department = x.Department,
                    Item = x.Item,
                    CustomsCertificateNumber = x.Customs_Certificate_Number,
                    NationalID = x.National_ID,
                    TaxRegistry = x.Tax_Registry,
                    CommercialRegister = x.Commercial_Register,
                    LedgerNumber = x.Ledger_Number,
                    Name = x.Name,
                    FarmName = x.FarmName,
                    BankCode = x.Code_Bank,
                    IsUsed = x.Is_Used,
                    UsedDate = x.Used_Date,
                    UsedByUserId = x.Used_By_User_Id,
                    UsedByUserName = x.Used_By_User_Name,
                    Details = x.Fees_Altahsil_Detiles
                        .OrderBy(d => d.ID)
                        .Select(d => new FeesAltahsilDetileDTO
                        {
                            Amount = d.Amount,
                            Quantity = d.Quantity,
                            FeeDescription = d.Fee_Description,
                            FeesTypeName = d.FeesType != null
                                ? d.FeesType.Name_Ar
                                : null
                        })
                        .ToList()
                })
                .ToList();

            var workbook = PaymentsExcelExporter.Create(
                payments,
                reportTitle);
            var fileName =
                $"{filePrefix}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            return File(
                workbook,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [HttpGet]
        public IActionResult PrintPayment(long id, int accountType)
        {
            if (HttpContext.Session.GetString("UserSession") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (accountType != 138 && accountType != 139)
            {
                return BadRequest();
            }

            using var context = new AgricultureDBContext();

            var receipt = context.Fees_Altahsils
                .AsNoTracking()
                .Where(x =>
                    x.ID == id &&
                    x.Account_Type == accountType &&
                    x.IsSuccess_Bank == true &&
                    x.Code_Bank == "00")
                .Select(x => new FeesAltahsilDTO
                {
                    ID = x.ID,
                    OrderNumber = x.OrderNumber,
                    Amount_Total = x.Amount_Total,
                    User_Creation_Date = x.User_Creation_Date,
                    Date = x.date,
                    Account_Type = x.Account_Type,
                    Office = x.office,
                    Department = x.Department,
                    Item = x.Item,
                    Customs_Certificate_Number = x.Customs_Certificate_Number,
                    National_ID = x.National_ID,
                    Tax_Registry = x.Tax_Registry,
                    Commercial_Register = x.Commercial_Register,
                    Ledger_Number = x.Ledger_Number,
                    Name = x.Name,
                    FarmName = x.FarmName,
                    Details = x.Fees_Altahsil_Detiles
                        .OrderBy(d => d.ID)
                        .Select(d => new FeesAltahsilDetileDTO
                        {
                            Amount = d.Amount,
                            Quantity = d.Quantity,
                            FeeDescription = d.Fee_Description,
                            FeesTypeName = d.FeesType != null
                                ? d.FeesType.Name_Ar
                                : null
                        })
                        .ToList()
                })
                .FirstOrDefault();

            if (receipt == null)
            {
                return NotFound();
            }

            ViewData["SuccessMessage"] = "تم الدفع بنجاح";
            ViewData["AutoPrint"] = true;

            return View("~/Views/Resit/Index.cshtml", receipt);
        }

        private IActionResult SuccessfulPayments(
            int accountType,
            string title,
            string actionName,
            string? search,
            DateTime? fromDate,
            DateTime? toDate,
            int page)
        {
            if (HttpContext.Session.GetString("UserSession") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            const int pageSize = 10;
            search = search?.Trim();

            using var context = new AgricultureDBContext();

            var query = context.Fees_Altahsils
                .AsNoTracking()
                .Where(x =>
                    x.Account_Type == accountType &&
                    x.IsSuccess_Bank == true &&
                    x.Code_Bank == "00");

            if (!string.IsNullOrWhiteSpace(search))
            {
                var hasNumericSearch = long.TryParse(search, out var numericSearch);

                query = query.Where(x =>
                    (hasNumericSearch && x.ID == numericSearch) ||
                    (x.OrderNumber != null && x.OrderNumber.Contains(search)) ||
                    x.National_ID.Contains(search) ||
                    (x.Name != null && x.Name.Contains(search)) ||
                    (x.office != null && x.office.Contains(search)) ||
                    (x.Department != null && x.Department.Contains(search)) ||
                    (x.Item != null && x.Item.Contains(search)) ||
                    (x.Ledger_Number != null &&
                     x.Ledger_Number.Contains(search)) ||
                    (x.Customs_Certificate_Number != null &&
                     x.Customs_Certificate_Number.Contains(search)));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x =>
                    x.User_Creation_Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date.AddDays(1);
                query = query.Where(x => x.User_Creation_Date < endDate);
            }

            var totalCount = query.Count();
            var filteredTotalAmount = query
                .Select(x => (decimal?)x.Amount_Total)
                .Sum() ?? 0m;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, page);

            if (totalPages > 0)
            {
                page = Math.Min(page, totalPages);
            }

            var payments = query
                .OrderByDescending(x => x.User_Creation_Date)
                .ThenByDescending(x => x.ID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SuccessfulPaymentVM
                {
                    ID = x.ID,
                    OrderNumber = x.OrderNumber,
                    CreationDate = x.User_Creation_Date,
                    PaymentDate = x.date,
                    TotalAmount = x.Amount_Total,
                    Office = x.office,
                    Department = x.Department,
                    Item = x.Item,
                    CustomsCertificateNumber = x.Customs_Certificate_Number,
                    NationalID = x.National_ID,
                    TaxRegistry = x.Tax_Registry,
                    CommercialRegister = x.Commercial_Register,
                    LedgerNumber = x.Ledger_Number,
                    Name = x.Name,
                    FarmName = x.FarmName,
                    BankCode = x.Code_Bank,
                    IsUsed = x.Is_Used,
                    UsedDate = x.Used_Date,
                    UsedByUserId = x.Used_By_User_Id,
                    UsedByUserName = x.Used_By_User_Name,
                    Details = x.Fees_Altahsil_Detiles
                        .OrderBy(d => d.ID)
                        .Select(d => new FeesAltahsilDetileDTO
                        {
                            Amount = d.Amount,
                            Quantity = d.Quantity,
                            FeeDescription = d.Fee_Description,
                            FeesTypeName = d.FeesType != null
                                ? d.FeesType.Name_Ar
                                : null
                        })
                        .ToList()
                })
                .ToList();

            var model = new SuccessfulPaymentsVM
            {
                Title = title,
                ActionName = actionName,
                AccountType = accountType,
                Search = search,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                FilteredTotalAmount = filteredTotalAmount,
                TotalPages = totalPages,
                Payments = payments
            };

            return View("SuccessfulPayments", model);
        }

        public IActionResult GeneralPayment()
        {
            if (!GovernmentPaymentsEnabled)
            {
                return RedirectToAction("Index", "Home");
            }

            AgricultureDBContext _context = new AgricultureDBContext();

            var model = new FeesAltahsilVM();

            model.Offices = GetOfficeNames(
                _context,
                canAcceptPaymentOnly: true);

            model.Fees = _context.FeesTypes
                .Where(x =>
                    x.IsActive &&
                    x.User_Deletion_Id == null &&
                    x.Display_Order.HasValue &&
                    x.Name_Ar != MechanizationFeeName &&
                    x.Full_Name != MechanizationFeeName &&
                    (x.Account_Type == 138 || x.Account_Type == 0))
                .OrderBy(x => x.Display_Order)
                .Select(x => new FeeVM
                {
                    FeesType_ID = x.ID,
                    FeesName = !string.IsNullOrWhiteSpace(x.Full_Name)
                        ? x.Full_Name
                        : x.Name_Ar,
                    Quantity = x.ID == MartyrFeeTypeId ||
                               x.ID == DifferenceFeeTypeId
                        ? 1
                        : 0,
                    Amount = x.Price,
                    IsFixedPrice = x.Price.HasValue
                })
                .ToList();

            model.Amount_Total = MartyrFeeMinimumAmount;

            ConfigurePaymentView(isGeneralPayment: true);
            return View("InspectionPayment", model);
        }

        public IActionResult InspectionPayment()
        {
            if (!PrivatePaymentsEnabled)
            {
                return RedirectToAction("Index", "Home");
            }

            AgricultureDBContext _context = new AgricultureDBContext();

            var model = new FeesAltahsilVM();
            model.Offices = GetOfficeNames(_context, canAcceptPaymentOnly: true);
            model.Fees = _context.FeesTypes
                .Where(x =>
                    x.IsActive &&
                    x.User_Deletion_Id == null &&
                    x.Display_Order.HasValue &&
                    x.Name_Ar != MechanizationFeeName &&
                    x.Full_Name != MechanizationFeeName &&
                    x.ID != LegacyPrivateMartyrFeeTypeId &&
                    (x.Account_Type == 139 ||
                     x.Account_Type == 0 ||
                     x.ID == MartyrFeeTypeId))
                .OrderBy(x => x.Display_Order)
                .Select(x => new FeeVM
                {
                    FeesType_ID = x.ID,
                    FeesName = !string.IsNullOrWhiteSpace(x.Full_Name)
                        ? x.Full_Name
                        : x.Name_Ar,
                    Quantity = x.ID == MartyrFeeTypeId ||
                               x.ID == DifferenceFeeTypeId
                        ? 1
                        : 0,
                    Amount = x.Price ?? (x.ID == MartyrFeeTypeId
                        ? MartyrFeeMinimumAmount
                        : null),
                    IsFixedPrice = x.Price.HasValue
                })
                .ToList();
            model.Amount_Total = MartyrFeeMinimumAmount;
            ConfigurePaymentView(isGeneralPayment: false);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveGeneralPayment(FeesAltahsilVM model)
        {
            if (!GovernmentPaymentsEnabled)
            {
                return RedirectToAction("Index", "Home");
            }

            AgricultureDBContext _context = new AgricultureDBContext();

            var governmentFeeDefinitions = _context.FeesTypes
                .Where(x =>
                    x.IsActive &&
                    x.User_Deletion_Id == null &&
                    x.Display_Order.HasValue &&
                    x.Name_Ar != MechanizationFeeName &&
                    x.Full_Name != MechanizationFeeName &&
                    (x.Account_Type == 138 || x.Account_Type == 0))
                .ToDictionary(x => x.ID);

            var mechanizationFeeDefinitions =
                GetMechanizationFeeDefinitions(_context);

            if (mechanizationFeeDefinitions.Count != 1)
            {
                ModelState.AddModelError(
                    "",
                    "إعداد رسوم الميكنة غير مكتمل. يرجى التواصل مع مسؤول النظام");
            }

            foreach (var fee in model.Fees ?? new List<FeeVM>())
            {
                if (governmentFeeDefinitions.TryGetValue(
                    fee.FeesType_ID,
                    out var feeDefinition))
                {
                    fee.FeesName = !string.IsNullOrWhiteSpace(feeDefinition.Full_Name)
                        ? feeDefinition.Full_Name
                        : feeDefinition.Name_Ar;
                    fee.IsFixedPrice = feeDefinition.Price.HasValue;

                    if (feeDefinition.Price.HasValue)
                    {
                        fee.Amount = feeDefinition.Price.Value;
                    }
                }

                if (fee.FeesType_ID == DifferenceFeeTypeId)
                {
                    fee.Quantity = 1;
                }

                fee.Selected = fee.Quantity > 0 && fee.Amount > 0;
            }

            ValidateRequiredMartyrFee(model, MartyrFeeTypeId);
            ValidateDifferenceFee(model);

            var allowedGovernmentFeeIds = governmentFeeDefinitions.Keys.ToHashSet();

            if ((model.Fees ?? new List<FeeVM>())
                .Any(x => x.Selected && !allowedGovernmentFeeIds.Contains(x.FeesType_ID)))
            {
                ModelState.AddModelError("", "يوجد رسم غير تابع للحساب الحكومي");
            }

            var officeCanAcceptPayment =
                !string.IsNullOrWhiteSpace(model.office) &&
                _context.Outlets.Any(x =>
                    x.Ar_Name == model.office &&
                    x.IsActive &&
                    x.User_Deletion_Id == null &&
                    x.CanAcceptPayment == true);

            if (!officeCanAcceptPayment)
            {
                ModelState.AddModelError(
                    nameof(model.office),
                    "المكتب المحدد غير متاح لاستقبال الدفع");
            }

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
                model.Offices = GetOfficeNames(
                    _context,
                    canAcceptPaymentOnly: true);
                ConfigurePaymentView(isGeneralPayment: true);
                return View("InspectionPayment", model);
            }

            var feesSubtotal = model.Fees
                .Where(x => x.Selected && x.Amount.HasValue)
                .Sum(x => x.IsFixedPrice
                    ? x.Amount!.Value * x.Quantity
                    : x.Amount!.Value);
            var mechanizationAmount = CalculateMechanizationFee(feesSubtotal);
            model.Amount_Total = feesSubtotal + mechanizationAmount;

            //save in database

            String Order_No = "7" + Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 9) + Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 3);

            Fees_Altahsil fe = new Fees_Altahsil
            {
                Amount_Total = model.Amount_Total,
                office = model.office,
                Department = model.Department,
                Item = model.Item,
                Name = model.Name,
                National_ID = model.National_ID,
                Customs_Certificate_Number = model.Customs_Certificate_Number,
                OrderNumber = Order_No,
                Account_Type = 138,
                Payment_Type_ID = 125,
                Ledger_Number = model.Ledger_Number,
                User_Creation_Date = DateTime.Now,
                FarmName=model.FarmName,
                date = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Fees_Altahsils.Add(fe);
            _context.SaveChanges(); 

            foreach (var item in model.Fees.Where(x =>
                x.Selected && x.Amount != null))
            {
                Fees_Altahsil_Detile det = new Fees_Altahsil_Detile
                {
                    FeesType_ID = item.FeesType_ID,
                    Amount = item.IsFixedPrice
                        ? item.Amount * item.Quantity
                        : item.Amount,
                    Quantity = item.Quantity,
                    Fee_Description = item.FeesType_ID == DifferenceFeeTypeId
                        ? item.FeeDescription?.Trim()
                        : null,
                    Fees_Altahsil_ID = fe.ID,
                    User_Creation_Date = DateTime.Now,
                };

                _context.Fees_Altahsil_Detiles.Add(det);
            }

            Fees_Altahsil_Detile mechanizationDetail = new Fees_Altahsil_Detile
            {
                FeesType_ID = mechanizationFeeDefinitions[0].ID,
                Amount = mechanizationAmount,
                Quantity = 1,
                Fees_Altahsil_ID = fe.ID,
                User_Creation_Date = DateTime.Now,
            };
            _context.Fees_Altahsil_Detiles.Add(mechanizationDetail);

            _context.SaveChanges();


            decimal amount = 0;
            HttpClient client = new HttpClient();

            //int? port = Request.Host.Port;
            //int? Bank_port = Request.Host.Port;
            //ViewBag.port = port;
            //ViewBag.ScriptBankPort = Bank_port;

            ///في حالة الدفع بالفيزا///



                //API insert Transaction and Transaction Details

              amount = model.Amount_Total;



                string Url = Request.Host.Host;
            
                Out_SessionDTO ss = new Out_SessionDTO();

                //string DomainName = Method_Bank.DomainName(port);

                //string host = Request.Host.Host;
                string Succesurl = "";
                string Cancelurl = "";
            //if (host == "site.capq.gov.eg")
            //{
            //    Succesurl = "https://site.capq.gov.eg/" + "ResitPayment/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
            //    Cancelurl = "https://site.capq.gov.eg/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";

            //}
            //else
            //{
            //    Succesurl = "http://localhost:5205/" + "ResitPayment/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
            //    Cancelurl = "http://localhost:5205/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";

            //}
            Succesurl = "https://site.capq.gov.eg/" + "Resit/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
            Cancelurl = "https://site.capq.gov.eg/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";

            ss = Method_Bank.Create_Session(amount, Order_No, Cancelurl, Succesurl/* Bank_port, host*/);
                ViewBag.Url = Url;

                ViewBag.Ses_Id = ss.Session_Id;
                ViewBag.Ord_Id = ss.Order_No;
                ViewBag.amount = ss.amount;
                return View();

                

         
        }

        private static List<string> GetOfficeNames(
            AgricultureDBContext context,
            bool canAcceptPaymentOnly = false)
        {
            var query = context.Outlets
                .Where(x =>
                    x.IsActive &&
                    x.Ar_Name != null &&
                    x.Ar_Name != "" &&
                    x.User_Deletion_Id == null);

            if (canAcceptPaymentOnly)
            {
                query = query.Where(x => x.CanAcceptPayment == true);
            }

            return query
                .Select(x => x.Ar_Name!)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        private void ConfigurePaymentView(bool isGeneralPayment)
        {
            ViewData["IsGeneralPayment"] = isGeneralPayment;
            ViewData["MechanizationFixedAmount"] = MechanizationFixedAmount;
            ViewData["MechanizationRate"] = MechanizationRate;
            ViewData["RequiredMartyrFeeTypeId"] = MartyrFeeTypeId;
            ViewData["DifferenceFeeTypeId"] = DifferenceFeeTypeId;
            ViewData["PaymentAction"] = isGeneralPayment
                ? nameof(SaveGeneralPayment)
                : nameof(SaveInspectionPayment);
            ViewData["PaymentTitle"] = isGeneralPayment
                ? "دفع رسوم الخدمات والشهادات"
                : "دفع رسوم الاعتمادات والعينات والبدلات";
            ViewData["PaymentDescription"] = isGeneralPayment
                ? "أدخل بيانات الطلب، ثم اختر الخدمات أو الشهادات المطلوبة وراجع الإجمالي قبل الدفع."
                : "أدخل بيانات الطلب، ثم اختر رسوم الاعتمادات أو العينات أو البدلات المطلوبة وراجع الإجمالي قبل الدفع.";
            ViewData["FeesSectionTitle"] = isGeneralPayment
                ? "الخدمات والشهادات"
                : "الاعتمادات والعينات والبدلات";
        }

        private static decimal CalculateMechanizationFee(decimal feesSubtotal)
        {
            var untrimmedAmount =
                MechanizationFixedAmount + (feesSubtotal * MechanizationRate);
            return Math.Truncate(untrimmedAmount * 100m) / 100m;
        }

        private static List<FeesType> GetMechanizationFeeDefinitions(
            AgricultureDBContext context)
        {
            return context.FeesTypes
                .Where(x =>
                    x.IsActive &&
                    x.User_Deletion_Id == null &&
                    x.Account_Type == 0 &&
                    (x.Name_Ar == MechanizationFeeName ||
                     x.Full_Name == MechanizationFeeName))
                .ToList();
        }

        private void ValidateRequiredMartyrFee(
            FeesAltahsilVM model,
            byte martyrFeeTypeId)
        {
            var martyrFeeIndex = model.Fees?.FindIndex(x =>
                x.FeesType_ID == martyrFeeTypeId) ?? -1;

            if (martyrFeeIndex < 0)
            {
                ModelState.AddModelError(
                    "",
                    "رسوم صندوق الشهداء إلزامية");
                return;
            }

            var martyrFee = model.Fees![martyrFeeIndex];

            if (martyrFee.Quantity < 1)
            {
                ModelState.AddModelError(
                    $"Fees[{martyrFeeIndex}].Quantity",
                    "عدد رسوم صندوق الشهداء يجب ألا يقل عن 1");
            }

            if (!martyrFee.Amount.HasValue ||
                martyrFee.Amount.Value < MartyrFeeMinimumAmount)
            {
                ModelState.AddModelError(
                    $"Fees[{martyrFeeIndex}].Amount",
                    "رسوم صندوق الشهداء إلزامية والحد الأدنى 5 جنيه");
            }
        }

        private void ValidateDifferenceFee(FeesAltahsilVM model)
        {
            var differenceFeeIndex = model.Fees?.FindIndex(x =>
                x.FeesType_ID == DifferenceFeeTypeId) ?? -1;

            if (differenceFeeIndex < 0)
            {
                return;
            }

            var differenceFee = model.Fees![differenceFeeIndex];
            differenceFee.Quantity = 1;

            if (differenceFee.Amount > 0 &&
                string.IsNullOrWhiteSpace(differenceFee.FeeDescription))
            {
                ModelState.AddModelError(
                    $"Fees[{differenceFeeIndex}].FeeDescription",
                    "بيان فرق الرسوم مطلوب عند إدخال القيمة");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveInspectionPayment(FeesAltahsilVM model)
        {
            if (!PrivatePaymentsEnabled)
            {
                return RedirectToAction("Index", "Home");
            }

			try
            {
				AgricultureDBContext _context = new AgricultureDBContext();

                var privateFeeDefinitions = _context.FeesTypes
                    .Where(x =>
                        x.IsActive &&
                        x.User_Deletion_Id == null &&
                        x.Name_Ar != MechanizationFeeName &&
                        x.Full_Name != MechanizationFeeName &&
                        x.ID != LegacyPrivateMartyrFeeTypeId &&
                        (x.Account_Type == 139 ||
                         x.Account_Type == 0 ||
                         x.ID == MartyrFeeTypeId))
                    .ToDictionary(x => x.ID);

                foreach (var fee in model.Fees ?? new List<FeeVM>())
                {
                    if (privateFeeDefinitions.TryGetValue(
                        fee.FeesType_ID,
                        out var feeDefinition))
                    {
                        fee.FeesName = !string.IsNullOrWhiteSpace(feeDefinition.Full_Name)
                            ? feeDefinition.Full_Name
                            : feeDefinition.Name_Ar;
                        fee.IsFixedPrice = feeDefinition.Price.HasValue;

                        if (feeDefinition.Price.HasValue)
                        {
                            fee.Amount = feeDefinition.Price.Value;
                        }
                    }

                    if (fee.FeesType_ID == DifferenceFeeTypeId)
                    {
                        fee.Quantity = 1;
                    }

                    fee.Selected = fee.Quantity > 0 && fee.Amount > 0;
                }

                ValidateRequiredMartyrFee(model, MartyrFeeTypeId);
                ValidateDifferenceFee(model);

                var allowedPrivateFeeIds = privateFeeDefinitions.Keys.ToHashSet();

                var mechanizationFeeDefinitions =
                    GetMechanizationFeeDefinitions(_context);

                if (mechanizationFeeDefinitions.Count != 1)
                {
                    ModelState.AddModelError(
                        "",
                        "إعداد رسوم الميكنة غير مكتمل. يرجى التواصل مع مسؤول النظام");
                }

                if ((model.Fees ?? new List<FeeVM>())
                    .Any(x => x.Selected && !allowedPrivateFeeIds.Contains(x.FeesType_ID)))
                {
                    ModelState.AddModelError("", "يوجد رسم غير تابع للحساب الخاص");
                }

                var officeCanAcceptPayment =
                    !string.IsNullOrWhiteSpace(model.office) &&
                    _context.Outlets.Any(x =>
                        x.Ar_Name == model.office &&
                        x.IsActive &&
                        x.User_Deletion_Id == null &&
                        x.CanAcceptPayment == true);

                if (!officeCanAcceptPayment)
                {
                    ModelState.AddModelError(
                        nameof(model.office),
                        "المكتب المحدد غير متاح لاستقبال مدفوعات الفحص");
                }

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
					model.Offices = GetOfficeNames(
                        _context,
                        canAcceptPaymentOnly: true);
                    ConfigurePaymentView(isGeneralPayment: false);
					return View("InspectionPayment", model);
				}

                var feesSubtotal = model.Fees
                    .Where(x => x.Selected && x.Amount.HasValue)
                    .Sum(x => x.Amount!.Value * x.Quantity);
                var mechanizationAmount = CalculateMechanizationFee(feesSubtotal);
                model.Amount_Total = feesSubtotal + mechanizationAmount;
				//save in database

				String Order_No = "8" + Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 9) + Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 3);

				Fees_Altahsil fe = new Fees_Altahsil
				{
					Amount_Total = model.Amount_Total,
					office = model.office,
					Department = model.Department,
					Item = model.Item,
					Name = model.Name,
					National_ID = model.National_ID,
					Customs_Certificate_Number = model.Customs_Certificate_Number,
					OrderNumber = Order_No,
					Account_Type = 139,
					Payment_Type_ID = 125,
					Ledger_Number = model.Ledger_Number,
					User_Creation_Date = DateTime.Now,
					FarmName = model.FarmName,
					date = DateOnly.FromDateTime(DateTime.Now)
				};

				_context.Fees_Altahsils.Add(fe);
				_context.SaveChanges();

				foreach (var item in model.Fees.Where(x =>
                    x.Selected && x.Amount != null))
				{
					Fees_Altahsil_Detile det = new Fees_Altahsil_Detile
					{
						FeesType_ID = item.FeesType_ID,
						Amount = item.Amount * item.Quantity,
						Quantity = item.Quantity,
                        Fee_Description = item.FeesType_ID == DifferenceFeeTypeId
                            ? item.FeeDescription?.Trim()
                            : null,
						Fees_Altahsil_ID = fe.ID,
						User_Creation_Date = DateTime.Now,
					};

					_context.Fees_Altahsil_Detiles.Add(det);
				}

                Fees_Altahsil_Detile mechanizationDetail = new Fees_Altahsil_Detile
                {
                    FeesType_ID = mechanizationFeeDefinitions[0].ID,
                    Amount = mechanizationAmount,
                    Quantity = 1,
                    Fees_Altahsil_ID = fe.ID,
                    User_Creation_Date = DateTime.Now,
                };
                _context.Fees_Altahsil_Detiles.Add(mechanizationDetail);

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

				//string DomainName = Method_Bank.DomainName(port);

				//string host = Request.Host.Host;

    //            _logger.LogInformation("Host: {host}", host);
    //            _logger.LogInformation("Session ID: {Session}", ss?.Session_Id);

                string Succesurl = "";
				string Cancelurl = "";
                //if (host == "10.10.21.12")
                //{
                //	Succesurl = "http://10.10.21.12:8071/" + "ResitPayment/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
                //	Cancelurl = "http://10.10.21.12:8071/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";
                //}
                //else if (host == "41.33.237.90")
                //{
                //	Succesurl = "http://41.33.237.90:8071/" + "ResitPayment/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
                //	Cancelurl = "http://41.33.237.90:8071/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";
                //}
                // if (host == "site.capq.gov.eg")
                //{
                //	Succesurl = "https://site.capq.gov.eg/" + "ResitPayment/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
                //	Cancelurl = "https://site.capq.gov.eg/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";

                //}
                //else
                //{
                //	Succesurl = "http://localhost:5205/" + "ResitPayment/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
                //	Cancelurl = "http://localhost:5205/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";

                //}
                Succesurl = "https://site.capq.gov.eg/" + "ResitPayment/Index?ID=" + fe.ID + "&Order_No=" + Order_No;
                Cancelurl = "https://site.capq.gov.eg/" + "Fees/Index/cancelorder?ID=" + fe.ID + "&Order_No=";

            //    string logPath = Path.Combine(
            //    AppContext.BaseDirectory,
            //    "payment_log.txt"
            //);

            //    System.IO.File.AppendAllText(
            //        logPath,
            //        $"Date: {DateTime.Now}\r\n" +
            //        $"port: {port}\r\n" +
            //        $"Bankport: {Bank_port}\r\n" +
            //        $"Host: {host}\r\n"


            //    );

                ss = Method_Bank.Create_SessionFor_Inspection(amount, Order_No, Cancelurl, Succesurl/*, Bank_port, host*/);

				
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
