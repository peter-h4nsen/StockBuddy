using System;

namespace StockBuddy.Domain.Entities
{
    public sealed class Position
    {
        public Position(int stockId, int quantity, decimal value)
        {
            StockId = stockId;
            Quantity = quantity;
            Value = value;
        }

        public int StockId { get; }
        public int Quantity { get; }
        public decimal Value { get; }
    }
}
