using StockBuddy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Domain.DTO
{
    public class HistoricalStockInfoResult
    {
        public HistoricalStockInfoResult(string symbol, HistoricalStockInfo[] stockInfoItems, bool isSuccess)
        {
            Symbol = symbol;
            StockInfoItems = stockInfoItems;
            IsSuccess = isSuccess;
        }

        public string Symbol { get; }
        public HistoricalStockInfo[] StockInfoItems { get; }
        public bool IsSuccess { get; }
    }
}
