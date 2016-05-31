using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Domain.DTO.YearlyReport
{
    public sealed class YearlyReportStockGroupDTO
    {
        public YearlyReportStockGroupDTO(
            string header, decimal profitLoss, bool isProfit,
            IEnumerable<YearlyReportStockGroupItemDTO> items)
        {
            Header = header;
            ProfitLoss = profitLoss;
            IsProfit = isProfit;
            Items = items;
        }

        public string Header { get; }
        public decimal ProfitLoss { get; }
        public bool IsProfit { get; }
        public IEnumerable<YearlyReportStockGroupItemDTO> Items { get; }
    }
}
