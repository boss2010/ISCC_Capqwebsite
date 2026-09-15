using CapqApI.PlantQuarantineData;
using CapqWebDTO;
using CapqModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CapqApI.Controllers
{
    [RoutePrefix("api/SuccessfulPayments")]
    public class SuccessfulPaymentsController : ApiController
    {
        [HttpGet]
        [Route("")]
        public HttpResponseMessage GetPayments(
            int accountType,
            string search = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 10)
        {
            if (!IsValidAccountType(accountType))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid account type.");
            }

            page = Math.Max(1, page);
            pageSize = Math.Max(1, Math.Min(pageSize, 50000));
            search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

            try
            {
                using (var db = new PlantQuarantineDataContext())
                {
                    var query = db.Fees_Altahsils.Where(x =>
                        x.Account_Type == accountType &&
                        x.IsSuccess_Bank == true &&
                        x.Code_Bank == "00");

                    if (search != null)
                    {
                        long numericSearch;
                        var isNumeric = long.TryParse(search, out numericSearch);

                        query = query.Where(x =>
                            (isNumeric && x.ID == numericSearch) ||
                            (x.OrderNumber != null && x.OrderNumber.Contains(search)) ||
                            (x.National_ID != null && x.National_ID.Contains(search)) ||
                            (x.Name != null && x.Name.Contains(search)) ||
                            (x.office != null && x.office.Contains(search)) ||
                            (x.Department != null && x.Department.Contains(search)) ||
                            (x.Item != null && x.Item.Contains(search)) ||
                            (x.Ledger_Number != null && x.Ledger_Number.Contains(search)) ||
                            (x.Customs_Certificate_Number != null && x.Customs_Certificate_Number.Contains(search)));
                    }

                    if (fromDate.HasValue)
                    {
                        var start = fromDate.Value.Date;
                        query = query.Where(x => x.User_Creation_Date >= start);
                    }

                    if (toDate.HasValue)
                    {
                        var end = toDate.Value.Date.AddDays(1);
                        query = query.Where(x => x.User_Creation_Date < end);
                    }

                    var totalCount = query.Count();
                    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                    if (totalPages > 0)
                    {
                        page = Math.Min(page, totalPages);
                    }

                    var rows = query
                        .OrderByDescending(x => x.User_Creation_Date)
                        .ThenByDescending(x => x.ID)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    var result = new SuccessfulPaymentsReportDTO
                    {
                        AccountType = accountType,
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        Payments = rows.Select(MapPayment).ToList()
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("{id:long}")]
        public HttpResponseMessage GetPayment(long id, int accountType)
        {
            if (!IsValidAccountType(accountType))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid account type.");
            }

            try
            {
                using (var db = new PlantQuarantineDataContext())
                {
                    var payment = db.Fees_Altahsils.FirstOrDefault(x =>
                        x.ID == id &&
                        x.Account_Type == accountType &&
                        x.IsSuccess_Bank == true &&
                        x.Code_Bank == "00");

                    if (payment == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Payment was not found.");
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, MapPayment(payment));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("{id:long}/mark-used")]
        public HttpResponseMessage MarkUsed(long id, int accountType, int usedByUserId)
        {
            if (!IsValidAccountType(accountType))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid account type.");
            }

            if (usedByUserId <= 0 || usedByUserId > short.MaxValue)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid user id.");
            }

            try
            {
                var usedByUserName = GetPrivilegeUserName(usedByUserId);
                if (string.IsNullOrWhiteSpace(usedByUserName))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The user was not found.");
                }

                using (var db = new PlantQuarantineDataContext())
                {
                    var usedDate = DateTime.Now;
                    var updatedRows = db.ExecuteCommand(
                        @"UPDATE dbo.Fees_Altahsil
                          SET Is_Used = 1,
                              Used_Date = {0},
                              Used_By_User_Id = {3},
                              Used_By_User_Name = {4}
                          WHERE ID = {1}
                            AND Account_Type = {2}
                            AND IsSuccess_Bank = 1
                            AND Code_Bank = '00'
                            AND Is_Used = 0",
                        usedDate,
                        id,
                        accountType,
                        (short)usedByUserId,
                        usedByUserName);

                    var result = new MarkPaymentUsedResultDTO
                    {
                        Updated = updatedRows == 1,
                        UsedDate = updatedRows == 1 ? (DateTime?)usedDate : null,
                        UsedByUserId = updatedRows == 1 ? (short?)usedByUserId : null,
                        UsedByUserName = updatedRows == 1 ? usedByUserName : null,
                        Message = updatedRows == 1
                            ? "تم تسجيل استخدام العملية بنجاح."
                            : "العملية مستخدمة بالفعل أو غير متاحة."
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static bool IsValidAccountType(int accountType)
        {
            return accountType == 138 || accountType == 139;
        }

        private static string GetPrivilegeUserName(int userId)
        {
            using (var privilegeDb = new DataClasses1DataContext())
            {
                return privilegeDb.DrawMenu(userId)
                    .Select(x => x.FullName)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            }
        }

        private static SuccessfulPaymentDTO MapPayment(Fees_Altahsil payment)
        {
            return new SuccessfulPaymentDTO
            {
                ID = payment.ID,
                OrderNumber = payment.OrderNumber,
                CreationDate = payment.User_Creation_Date,
                PaymentDate = payment.date,
                TotalAmount = payment.Amount_Total,
                AccountType = payment.Account_Type,
                Office = payment.office,
                Department = payment.Department,
                Item = payment.Item,
                CustomsCertificateNumber = payment.Customs_Certificate_Number,
                NationalID = payment.National_ID,
                LedgerNumber = payment.Ledger_Number,
                Name = payment.Name,
                FarmName = payment.FarmName,
                BankCode = payment.Code_Bank,
                IsUsed = payment.Is_Used,
                UsedDate = payment.Used_Date,
                UsedByUserId = payment.Used_By_User_Id,
                UsedByUserName = payment.Used_By_User_Name,
                Details = payment.Fees_Altahsil_Detiles
                    .OrderBy(x => x.ID)
                    .Select(x => new SuccessfulPaymentDetailDTO
                    {
                        ID = x.ID,
                        FeesTypeName = x.FeesType == null ? null : x.FeesType.Name_Ar,
                        Quantity = x.Quantity,
                        Amount = x.Amount
                    })
                    .ToList()
            };
        }
    }
}
