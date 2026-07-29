using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class FeesAltahsilDTO
    {
        public long ID { get; set; }

        public string? OrderNumber { get; set; }

        public decimal Amount_Total { get; set; }

        public DateTime User_Creation_Date { get; set; }
        public DateOnly? Date { get; set; }
        public string? Office { get; set; }

        public string? Customs_Certificate_Number { get; set; }

        public string? National_ID { get; set; }

        public string? Tax_Registry { get; set; }

        public string? Commercial_Register { get; set; }

        public string? Ledger_Number { get; set; }

        public string? Name { get; set; }
        public string? FarmName { get; set; }

        public List<FeesAltahsilDetileDTO> Details { get; set; }= new();
    }
    public class FeesAltahsilDetileDTO
    {

        public decimal? Amount { get; set; }

        public string? FeesTypeName { get; set; }
    }
}
