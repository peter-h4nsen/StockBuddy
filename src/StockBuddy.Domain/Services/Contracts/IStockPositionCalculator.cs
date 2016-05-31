using System;
using System.Collections.Generic;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Domain.Services.Contracts
{
    public interface IStockPositionCalculator
    {
        IEnumerable<Position> GetPositions(Deposit deposit);
        IEnumerable<StockHolding> GetStockHoldings(Deposit deposit);
        StockHolding GetStockHolding(Deposit deposit, int stockId, DateTime date);
        int CalculateTotalQuantity(IEnumerable<Trade> trades);
        decimal CalculateMarketValue(int quantity, decimal price);
    }
}
