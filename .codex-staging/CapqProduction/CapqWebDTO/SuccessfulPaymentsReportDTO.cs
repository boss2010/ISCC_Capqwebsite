using System;
using System.Collections.Generic;

namespace CapqWebDTO
{
    public class SuccessfulPaymentsReportDTO
    {
        public SuccessfulPaymentsReportDTO()
        {
            Payments = new List<SuccessfulPaymentDTO>();
        }

        public int AccountType { get; set; }
        public string Title { get; set; }
        public string ActionName { get; set; }
        public string Search { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<SuccessfulPaymentDTO> Payments { get; set; }
    }

    public class SuccessfulPaymentDTO
    {
        public SuccessfulPaymentDTO()
        {
            Details = new List<SuccessfulPaymentDetailDTO>();
        }

        public long ID { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int? AccountType { get; set; }
        public string Office { get; set; }
        public string Department { get; set; }
        public string Item { get; set; }
        public string CustomsCertificateNumber { get; set; }
        public string NationalID { get; set; }
        public string LedgerNumber { get; set; }
        public string Name { get; set; }
        public string FarmName { get; set; }
        public string BankCode { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedDate { get; set; }
        public short? UsedByUserId { get; set; }
        public string UsedByUserName { get; set; }
        public List<SuccessfulPaymentDetailDTO> Details { get; set; }
    }

    public class SuccessfulPaymentDetailDTO
    {
        public long ID { get; set; }
        public string FeesTypeName { get; set; }
        public int? Quantity { get; set; }
        public decimal? Amount { get; set; }
    }

    public class MarkPaymentUsedResultDTO
    {
        public bool Updated { get; set; }
        public string Message { get; set; }
        public DateTime? UsedDate { get; set; }
        public short? UsedByUserId { get; set; }
        public string UsedByUserName { get; set; }
    }
}
