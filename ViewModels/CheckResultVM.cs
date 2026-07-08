using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class CheckResultVM
    {
        public int SuccessCount { get; set; }

        public List<PaymentResultVM> Details { get; set; }
    }

    public class PaymentResultVM
    {
        public long ID { get; set; }

        public decimal Amount_Total { get; set; }
        public DateOnly? date { get; set; }

    }
}
