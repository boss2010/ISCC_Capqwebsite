using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class ExportingProcedureVM
    {
        public long IDInitiator { get; set; }
        public string ?CountryName { get; set; }
        public string ?itemName { get; set; }
        public string? ShortName_Ar { get; set; }
        public string? ConstrainText { get; set; }
        public short? Country_Id { get; set; }
        public long? Item_ShortName_ID { get; set; }
        public long? Item_ID { get; set; }
        public string? InSide_Certificate_Ar { get; set; }


        public string? ConstrainText_Ar { get; set; }

        public string? ConstrainText_En { get; set; }

   

        public string? InSide_Certificate_En { get; set; }

    }
}
