using System;
using System.Collections.Generic;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Domain.Services.Contracts
{
    public interface IStockPositionCalculator
    {
        IEnumerable<StockPosition> GetStockPositions(Deposit deposit);
        StockPosition GetStockPosition(Deposit deposit, int stockId, DateTime date);
        decimal CalculateMarketValue(int quantity, decimal price);
    }
}
