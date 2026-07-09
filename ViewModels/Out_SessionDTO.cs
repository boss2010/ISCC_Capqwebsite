using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class Out_SessionDTO
    {
        public string Session_Id { get; set; }

        public string ErrorMess { get; set; }
        public string Order_No { get; set; }
        public decimal amount { get; set; }
    }
}
