using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class DashboardVM
    {
        public IEnumerable<CountriesVM> Countries { get; set; }
        public IEnumerable<CountriesExVM> CountriesEx { get; set; }
        public IEnumerable<ProductsVM> Products { get; set; }
        public IEnumerable<ProductsEXVM> ProductsEX { get; set; }
    }

    public class CountriesVM
    { 
        public string Country { get; set; }
        public double CountOrders { get; set; }        
    }
    public class CountriesExVM
    {
        public string Country { get; set; }
        public double CountOrders { get; set; }
    }
    public class ProductsVM
    {
        public string Country { get; set; }
        public double CountOrders { get; set; }
    }
    public class ProductsEXVM
    {
        public string Country { get; set; }
        public double CountOrders { get; set; }
    }

}
