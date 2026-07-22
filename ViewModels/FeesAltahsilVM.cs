using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class FeesAltahsilVM
    {
        [Required(ErrorMessage = "\u0627\u0644\u0645\u0643\u062A\u0628 \u0645\u0637\u0644\u0648\u0628")]
        public string? office { get; set; }

        public List<string> Offices { get; set; } = new();

        public string? Customs_Certificate_Number { get; set; }

        [Required(ErrorMessage = "الرقم القومي مطلوب")]
        [RegularExpression(@"^\d{14}$",
            ErrorMessage =
            "الرقم القومي يجب أن يكون 14 رقم")]
        public string National_ID { get; set; }

        public string? Tax_Registry { get; set; }

        public string? Commercial_Register { get; set; }

        [Required(ErrorMessage = "اسم الشركة مطلوب")]
        public string? Name { get; set; }
        public decimal Amount_Total { get; set; }
        public string? FarmName { get; set; }
        public List<FeeVM> Fees { get; set; } = new();
    }

    public class FeeVM
    {
        public byte FeesType_ID { get; set; }

        public string FeesName { get; set; }

        public bool Selected { get; set; }

        [Range(
            0.01,
            999999999,
            ErrorMessage =
            "قيمة الرسوم يجب أن تكون أكبر من صفر")]
        public decimal? Amount { get; set; }
    }
}
