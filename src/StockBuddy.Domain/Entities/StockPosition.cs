using System;

namespace StockBuddy.Domain.Entities
{
    public sealed class StockPosition
    {
        public StockPosition(int stockId, int quantity)
        {
            StockId = stockId;
            Quantity = quantity;
        }

        public int StockId { get; }
        public int Quantity { get; }

        
        //TODO: Need to know the current value of this position. Will be calculated from realtime price.
        //public decimal MarketValue { get; }
    }
}
