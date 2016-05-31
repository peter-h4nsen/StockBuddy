using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Domain.DTO.YearlyReport
{
    public sealed class YearlyReportDividendDTO
    {
        public YearlyReportDividendDTO(
            DateTime date, string description, string stockType, int quantity, decimal dividendRate,
            decimal grossDividendPayment, decimal taxPayment, decimal netDividendPayment)
        {
            Date = date;
            Description = description;
            StockType = stockType;
            Quantity = quantity;
            DividendRate = dividendRate;
            GrossDividendPayment = grossDividendPayment;
            TaxPayment = taxPayment;
            NetDividendPayment = netDividendPayment;
        }

        public DateTime Date { get; }
        public string Description { get; }
        public string StockType { get; }
        public int Quantity { get; }
        public decimal DividendRate { get; }
        public decimal GrossDividendPayment { get; }
        public decimal TaxPayment { get; }
        public decimal NetDividendPayment { get; }

        public string FilterValue
        {
            get { return Description.ToLower(); }
        }
    }
}
