namespace ViewModels
{
    public class SuccessfulPaymentsVM
    {
        public string Title { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public int AccountType { get; set; }
        public string? Search { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<SuccessfulPaymentVM> Payments { get; set; } = new();
    }

    public class SuccessfulPaymentVM
    {
        public long ID { get; set; }
        public string? OrderNumber { get; set; }
        public DateTime CreationDate { get; set; }
        public DateOnly? PaymentDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Office { get; set; }
        public string? CustomsCertificateNumber { get; set; }
        public string? NationalID { get; set; }
        public string? TaxRegistry { get; set; }
        public string? CommercialRegister { get; set; }
        public string? Name { get; set; }
        public string? FarmName { get; set; }
        public string? BankCode { get; set; }
        public List<FeesAltahsilDetileDTO> Details { get; set; } = new();
    }
}
