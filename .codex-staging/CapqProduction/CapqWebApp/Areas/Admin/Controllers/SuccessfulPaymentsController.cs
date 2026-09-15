using CapqWebDTO;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CapqWebApp.Areas.Admin.Controllers
{
    public class SuccessfulPaymentsController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("GovernmentPayments");
        }

        [HttpGet]
        public ActionResult GovernmentPayments(string search, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            return LoadReport(138, "عمليات الدفع الحكومي الناجحة", "GovernmentPayments", search, fromDate, toDate, page, 10);
        }

        [HttpGet]
        public ActionResult PrivatePayments(string search, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            return LoadReport(139, "عمليات الدفع الخاص الناجحة", "PrivatePayments", search, fromDate, toDate, page, 10);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkPaymentUsed(
            long id,
            int accountType,
            string search,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            if (!HasPaymentPermission())
            {
                return LoginRedirect();
            }

            if (!IsValidAccountType(accountType))
            {
                return new HttpStatusCodeResult(400);
            }

            int usedByUserId;
            if (!int.TryParse(Convert.ToString(Session["IDSource"]), out usedByUserId))
            {
                return LoginRedirect();
            }

            try
            {
                using (var client = CreateClient())
                {
                    var url = GetApiBaseUrl() + "/api/SuccessfulPayments/" + id
                        + "/mark-used?accountType=" + accountType
                        + "&usedByUserId=" + usedByUserId;
                    var response = client.PostAsync(url, new StringContent(string.Empty)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var result = Deserialize<MarkPaymentUsedResultDTO>(response);
                        TempData[result.Updated ? "UsageSuccess" : "UsageInfo"] = result.Message;
                    }
                    else
                    {
                        TempData["UsageInfo"] = "تعذر تسجيل استخدام العملية. برجاء المحاولة مرة أخرى.";
                    }
                }
            }
            catch
            {
                TempData["UsageInfo"] = "تعذر الاتصال بخدمة الدفع. برجاء المحاولة مرة أخرى.";
            }

            return RedirectToAction(
                accountType == 138 ? "GovernmentPayments" : "PrivatePayments",
                new
                {
                    search = search,
                    fromDate = fromDate.HasValue ? fromDate.Value.ToString("yyyy-MM-dd") : null,
                    toDate = toDate.HasValue ? toDate.Value.ToString("yyyy-MM-dd") : null,
                    page = Math.Max(1, page)
                });
        }

        [HttpGet]
        public ActionResult PrintPayment(long id, int accountType)
        {
            if (!HasPaymentPermission())
            {
                return LoginRedirect();
            }

            if (!IsValidAccountType(accountType))
            {
                return new HttpStatusCodeResult(400);
            }

            try
            {
                using (var client = CreateClient())
                {
                    var url = GetApiBaseUrl() + "/api/SuccessfulPayments/" + id + "?accountType=" + accountType;
                    var response = client.GetAsync(url).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        return HttpNotFound();
                    }

                    var payment = Deserialize<SuccessfulPaymentDTO>(response);
                    ViewBag.AutoPrint = true;
                    return View("PrintPayment", payment);
                }
            }
            catch
            {
                return HttpNotFound();
            }
        }

        [HttpGet]
        public ActionResult ExportGovernmentPayments(string search, DateTime? fromDate, DateTime? toDate)
        {
            return ExportPayments(138, "عمليات الدفع الحكومي الناجحة", "GovernmentPayments", search, fromDate, toDate);
        }

        [HttpGet]
        public ActionResult ExportPrivatePayments(string search, DateTime? fromDate, DateTime? toDate)
        {
            return ExportPayments(139, "عمليات الدفع الخاص الناجحة", "PrivatePayments", search, fromDate, toDate);
        }

        private ActionResult LoadReport(
            int accountType,
            string title,
            string actionName,
            string search,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize)
        {
            if (!HasPaymentPermission())
            {
                return LoginRedirect();
            }

            var model = new SuccessfulPaymentsReportDTO
            {
                AccountType = accountType,
                Title = title,
                ActionName = actionName,
                Search = search,
                FromDate = fromDate,
                ToDate = toDate,
                Page = Math.Max(1, page),
                PageSize = pageSize
            };

            try
            {
                using (var client = CreateClient())
                {
                    var response = client.GetAsync(BuildReportUrl(accountType, search, fromDate, toDate, page, pageSize)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        model = Deserialize<SuccessfulPaymentsReportDTO>(response);
                        model.Title = title;
                        model.ActionName = actionName;
                        model.Search = search;
                        model.FromDate = fromDate;
                        model.ToDate = toDate;
                    }
                    else
                    {
                        ViewBag.ReportError = "تعذر تحميل تقرير المدفوعات.";
                    }
                }
            }
            catch
            {
                ViewBag.ReportError = "تعذر الاتصال بخدمة الدفع.";
            }

            return View("Index", model);
        }

        private ActionResult ExportPayments(
            int accountType,
            string title,
            string filePrefix,
            string search,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (!HasPaymentPermission())
            {
                return LoginRedirect();
            }

            try
            {
                using (var client = CreateClient())
                {
                    var response = client.GetAsync(BuildReportUrl(accountType, search, fromDate, toDate, 1, 50000)).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["UsageInfo"] = "تعذر تصدير التقرير.";
                        return RedirectToAction(accountType == 138 ? "GovernmentPayments" : "PrivatePayments");
                    }

                    var report = Deserialize<SuccessfulPaymentsReportDTO>(response);
                    var html = BuildExcelHtml(title, report);
                    var preamble = Encoding.UTF8.GetPreamble();
                    var body = Encoding.UTF8.GetBytes(html);
                    var bytes = new byte[preamble.Length + body.Length];
                    Buffer.BlockCopy(preamble, 0, bytes, 0, preamble.Length);
                    Buffer.BlockCopy(body, 0, bytes, preamble.Length, body.Length);

                    return File(
                        bytes,
                        "application/vnd.ms-excel",
                        filePrefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xls");
                }
            }
            catch
            {
                TempData["UsageInfo"] = "تعذر الاتصال بخدمة الدفع أثناء التصدير.";
                return RedirectToAction(accountType == 138 ? "GovernmentPayments" : "PrivatePayments");
            }
        }

        private string BuildReportUrl(
            int accountType,
            string search,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize)
        {
            var query = new StringBuilder();
            query.Append("?accountType=").Append(accountType);
            query.Append("&page=").Append(Math.Max(1, page));
            query.Append("&pageSize=").Append(pageSize);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query.Append("&search=").Append(HttpUtility.UrlEncode(search.Trim()));
            }

            if (fromDate.HasValue)
            {
                query.Append("&fromDate=").Append(fromDate.Value.ToString("yyyy-MM-dd"));
            }

            if (toDate.HasValue)
            {
                query.Append("&toDate=").Append(toDate.Value.ToString("yyyy-MM-dd"));
            }

            return GetApiBaseUrl() + "/api/SuccessfulPayments" + query;
        }

        private static string BuildExcelHtml(string title, SuccessfulPaymentsReportDTO report)
        {
            var html = new StringBuilder();
            html.Append("<html dir='rtl'><head><meta charset='utf-8'></head><body>");
            html.Append("<h2>").Append(HttpUtility.HtmlEncode(title)).Append("</h2>");
            html.Append("<table border='1'><thead><tr>");
            html.Append("<th>رقم العملية</th><th>رقم الطلب</th><th>التاريخ</th><th>الاسم / الشركة</th>");
            html.Append("<th>الرقم القومي</th><th>المكتب</th><th>القسم</th><th>الصنف</th>");
            html.Append("<th>رقم الشهادة / طلب الفحص / العينة</th><th>الإجمالي</th><th>حالة الاستخدام</th><th>تاريخ الاستخدام</th><th>تم الاستخدام بواسطة</th>");
            html.Append("</tr></thead><tbody>");

            foreach (var payment in report.Payments)
            {
                html.Append("<tr>");
                AppendCell(html, payment.ID.ToString());
                AppendCell(html, payment.OrderNumber);
                AppendCell(html, payment.CreationDate.ToString("dd/MM/yyyy hh:mm tt"));
                AppendCell(html, payment.Name);
                AppendCell(html, payment.NationalID);
                AppendCell(html, payment.Office);
                AppendCell(html, payment.Department);
                AppendCell(html, payment.Item);
                AppendCell(html, payment.CustomsCertificateNumber);
                AppendCell(html, payment.TotalAmount.ToString("0.####"));
                AppendCell(html, payment.IsUsed ? "تم الاستخدام" : "غير مستخدم");
                AppendCell(html, payment.UsedDate.HasValue ? payment.UsedDate.Value.ToString("dd/MM/yyyy hh:mm tt") : string.Empty);
                AppendCell(html, payment.UsedByUserName ?? (payment.UsedByUserId.HasValue ? payment.UsedByUserId.Value.ToString() : string.Empty));
                html.Append("</tr>");
            }

            html.Append("</tbody></table></body></html>");
            return html.ToString();
        }

        private static void AppendCell(StringBuilder html, string value)
        {
            html.Append("<td>").Append(HttpUtility.HtmlEncode(value ?? string.Empty)).Append("</td>");
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private static T Deserialize<T>(HttpResponseMessage response)
        {
            var json = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(json);
        }

        private string GetApiBaseUrl()
        {
            return APIConfiguration.GetApIUrl(Request.Url.Port).TrimEnd('/');
        }

        private bool HasPaymentPermission()
        {
            return Session["Payment"] != null && Session["IDSource"] != null;
        }

        private ActionResult LoginRedirect()
        {
            return RedirectToAction("Index", "Intro2", new { area = "Home" });
        }

        private static bool IsValidAccountType(int accountType)
        {
            return accountType == 138 || accountType == 139;
        }
    }
}
