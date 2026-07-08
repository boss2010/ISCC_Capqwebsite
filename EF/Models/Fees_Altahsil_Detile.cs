using System;
using System.Collections.Generic;

namespace EF.Models;

public partial class Fees_Altahsil_Detile
{
    public long ID { get; set; }

    public byte? FeesType_ID { get; set; }

    public long? Fees_Altahsil_ID { get; set; }

    /// <summary>
    /// المبلغ
    /// </summary>
    public decimal? Amount { get; set; }

    public DateTime User_Creation_Date { get; set; }

    public virtual FeesType? FeesType { get; set; }

    public virtual Fees_Altahsil? Fees_Altahsil { get; set; }
}
