using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Domain.DTO.YearlyReport
{
    public sealed class YearlyReportDTO
    {
        public YearlyReportDTO(
            decimal profit, decimal loss, decimal profitLoss, bool isProfit,
            decimal dividendDanishStocks, decimal dividendForeignStocks, decimal dividendInvestmentFonds,
            decimal grossReturn, string grossReturnDescription, decimal taxPayment, string taxPaymentDescription,
            decimal netReturn, bool isPositiveReturn, decimal lossDeduction,
            IEnumerable<YearlyReportStockGroupDTO> stockGroups, IEnumerable<YearlyReportDividendDTO> dividends)
        {
            Profit = profit;
            Loss = loss;
            ProfitLoss = profitLoss;
            IsProfit = isProfit;

            DividendDanishStocks = dividendDanishStocks;
            DividendForeignStocks = dividendForeignStocks;
            DividendInvestmentFonds = dividendInvestmentFonds;

            GrossReturn = grossReturn;
            GrossReturnDescription = grossReturnDescription;
            TaxPayment = taxPayment;
            TaxPaymentDescription = taxPaymentDescription;
            NetReturn = netReturn;
            IsPositiveReturn = isPositiveReturn;
            LossDeduction = lossDeduction;

            StockGroups = stockGroups;
            Dividends = dividends;
        }

        public decimal Profit { get; }
        public decimal Loss { get; }
        public decimal ProfitLoss { get; }
        public bool IsProfit { get; }

        public decimal DividendDanishStocks { get; }
        public decimal DividendForeignStocks { get; }
        public decimal DividendInvestmentFonds { get; }

        public decimal GrossReturn { get; }
        public string GrossReturnDescription { get; }
        public decimal TaxPayment { get; }
        public string TaxPaymentDescription { get; }
        public decimal NetReturn { get; }
        public bool IsPositiveReturn { get; }
        public decimal LossDeduction { get; }

        public IEnumerable<YearlyReportStockGroupDTO> StockGroups { get; }
        public IEnumerable<YearlyReportDividendDTO> Dividends { get; }
    }
}
