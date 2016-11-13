using System;
using System.Collections.Generic;
using System.Linq;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Services.Contracts;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Domain.Services.Impl
{
    public sealed class StockPositionCalculator : IStockPositionCalculator
    {

        public StockPosition GetStockPosition(Deposit deposit, int stockId, DateTime date)
        {
            Guard.AgainstNull(() => deposit);

            var quantity = 
                deposit.Trades
                .Where(p => p.TradeDate.Date <= date.Date && p.Stock.Splitted.Id == stockId)
                .Sum(t => t.QuantitySignedSplitted(date));

            return new StockPosition(stockId, quantity);
        }

        public IEnumerable<StockPosition> GetStockPositions(Deposit deposit)
        {
            var stockPositions =
                from trade in deposit.Trades
                group trade by trade.Stock.Splitted.Id into g
                let quantity = g.Sum(t => t.QuantitySignedSplitted())
                where quantity > 0
                select new StockPosition(g.Key, quantity);

            return stockPositions;
        }

        public decimal CalculateMarketValue(int quantity, decimal price)
        {
            return quantity * price;
        }
    }
}
