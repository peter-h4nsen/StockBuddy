﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Domain.Settings
{
    public class GlobalSettings
    {
        public Dictionary<int, TaxValues> YearlyTaxValues { get; set; }
    }
}
