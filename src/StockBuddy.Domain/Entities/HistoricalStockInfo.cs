using System;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Domain.Entities
{
    public sealed class HistoricalStockInfo : Entity
    {
        private HistoricalStockInfo()
        { }

        public HistoricalStockInfo(string symbol, DateTime date, decimal close)
        {
            Guard.AgainstNull(() => symbol);

            Symbol = symbol;
            Date = date;
            Close = close;
        }

        public string Symbol { get; private set; }
        public DateTime Date { get; private set; }
        public decimal Close { get; private set; }
    }
}
