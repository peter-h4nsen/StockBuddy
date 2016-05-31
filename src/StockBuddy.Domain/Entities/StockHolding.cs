using System;

namespace StockBuddy.Domain.Entities
{
    public sealed class StockHolding
    {
        public StockHolding(int stockId, int quantity)
        {
            StockId = stockId;
            Quantity = quantity;
        }

        public int StockId { get; }
        public int Quantity { get; }
    }
}
