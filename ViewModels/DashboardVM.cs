using System;
using System.Collections.Generic;

namespace ViewModels
{
	public class DashboardVM
	{
		// ==============================
		// Countries
		// ==============================

		public List<CountriesVM> Countries { get; set; }
			= new List<CountriesVM>();

		public List<CountriesExVM> CountriesEx { get; set; }
			= new List<CountriesExVM>();


		// ==============================
		// Products
		// ==============================

		public List<ProductsVM> Products { get; set; }
			= new List<ProductsVM>();

		public List<ProductsEXVM> ProductsEX { get; set; }
			= new List<ProductsEXVM>();


		// ==============================
		// Import
		// ==============================

		public double TotalImportTons { get; set; }

		public int ImportOrders { get; set; }


		// ==============================
		// Export
		// ==============================

		public double TotalExportTons { get; set; }

		public int ExportOrders { get; set; }


		// ==============================
		// Total
		// ==============================

		public double TotalTons
		{
			get
			{
				return TotalImportTons + TotalExportTons;
			}
		}


		// ==============================
		// Percentages
		// ==============================

		public double ImportPercentage
		{
			get
			{
				if (TotalTons <= 0)
					return 0;

				return Math.Round(
					(TotalImportTons / TotalTons) * 100,
					0
				);
			}
		}

		public double ExportPercentage
		{
			get
			{
				if (TotalTons <= 0)
					return 0;

				return Math.Round(
					(TotalExportTons / TotalTons) * 100,
					0
				);
			}
		}
	}


	// =========================================
	// Countries - Import
	// =========================================

	public class CountriesVM
	{
		public string Country { get; set; }

		public double CountOrders { get; set; }
	}


	// =========================================
	// Countries - Export
	// =========================================

	public class CountriesExVM
	{
		public string Country { get; set; }

		public double CountOrders { get; set; }
	}


	// =========================================
	// Products - Import
	// =========================================

	public class ProductsVM
	{
		public string Country { get; set; }

		public double CountOrders { get; set; }
	}


	// =========================================
	// Products - Export
	// =========================================

	public class ProductsEXVM
	{
		public string Country { get; set; }

		public double CountOrders { get; set; }
	}
}
