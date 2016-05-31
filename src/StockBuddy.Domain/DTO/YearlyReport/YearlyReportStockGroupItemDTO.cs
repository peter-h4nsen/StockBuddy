using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Domain.DTO.YearlyReport
{
    public sealed class YearlyReportStockGroupItemDTO
    {
        public YearlyReportStockGroupItemDTO(
            string description, DateTime date, int quantity, decimal sellValue, decimal buyValue,
            decimal profitLoss, bool isProfit, bool isSale)
        {
            Description = description;
            Date = date;
            Quantity = quantity;
            SellValue = sellValue;
            BuyValue = buyValue;
            ProfitLoss = profitLoss;
            IsProfit = isProfit;
            IsSale = isSale;
        }

        public string Description { get; }
        public DateTime Date { get; }
        public int Quantity { get; }
        public decimal SellValue { get; }
        public decimal BuyValue { get; }
        public decimal ProfitLoss { get; }
        public bool IsProfit { get; }
        public bool IsSale { get; }
    }
}
