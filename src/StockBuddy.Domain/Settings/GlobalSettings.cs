using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Domain.Settings
{
    public sealed class GlobalSettings
    {
        public decimal LowTaxLimit { get; set; }
        public decimal LowTaxRate { get; set; }
        public decimal HighTaxRate { get; set; }
    }
}
