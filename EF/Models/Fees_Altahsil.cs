using System;
using System.Collections.Generic;

namespace EF.Models;

public partial class Fees_Altahsil
{
    public long ID { get; set; }

    /// <summary>
    /// المبلغ
    /// </summary>
    public decimal Amount_Total { get; set; }

    public DateTime User_Creation_Date { get; set; }

    /// <summary>
    /// from systemcode table 30
    /// نوع عملية الدفع فيزا - كاش
    /// </summary>
    public int? Payment_Type_ID { get; set; }

    public string? OrderNumber { get; set; }

    /// <summary>
    /// 0 تم رفض عملية البنك
    /// 1 تم قبول العملية 
    /// null تم الارسال ولم الرد من البنك
    /// </summary>
    public bool? IsSuccess_Bank { get; set; }

    /// <summary>
    /// كود العملية من البنك
    /// </summary>
    public string? Code_Bank { get; set; }

    public DateOnly? date { get; set; }

    /// <summary>
    /// نوع الحساب من system code رقم 33
    /// </summary>
    public int? Account_Type { get; set; }

    public string? office { get; set; }

    public string? Customs_Certificate_Number { get; set; }

    public string National_ID { get; set; } = null!;

    public string? Tax_Registry { get; set; }

    public string? Commercial_Register { get; set; }

    public string? Name { get; set; }

    public string? FarmName { get; set; }

    public virtual A_SystemCode? Account_TypeNavigation { get; set; }

    public virtual ICollection<Fees_Altahsil_Detile> Fees_Altahsil_Detiles { get; set; } = new List<Fees_Altahsil_Detile>();

    public virtual A_SystemCode? Payment_Type { get; set; }
}
