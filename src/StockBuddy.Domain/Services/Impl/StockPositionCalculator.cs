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
        public IEnumerable<Position> GetPositions(Deposit deposit)
        {
            if (!deposit.Trades.Any())
                yield break;

            var stockHoldings = GetStockHoldings(deposit);

            foreach (var stockHolding in stockHoldings)
            {
                // TODO: Find kurs for papiret.
                var price = 150;
                var value = CalculateMarketValue(stockHolding.Quantity, price);

                var position = new Position(stockHolding.StockId, stockHolding.Quantity, value);
                yield return position;
            }
        }

        public StockHolding GetStockHolding(Deposit deposit, int stockId, DateTime date)
        {
            Guard.AgainstNull(() => deposit);

            var quantity = 
                deposit.Trades
                .Where(p => p.TradeDate.Date <= date.Date && p.Stock.Splitted.Id == stockId)
                .Sum(t => t.QuantitySignedSplitted(date));

            return new StockHolding(stockId, quantity);
        }

        public IEnumerable<StockHolding> GetStockHoldings(Deposit deposit)
        {
            var stockHoldings =
                from trade in deposit.Trades
                group trade by trade.Stock.Splitted.Id into g
                let quantity = CalculateTotalQuantity(g)
                where quantity > 0
                select new StockHolding(g.Key, quantity);

            return stockHoldings;
        }

        public int CalculateTotalQuantity(IEnumerable<Trade> trades)
        {
            return trades.Sum(t => t.QuantitySignedSplitted());
        }

        public decimal CalculateMarketValue(int quantity, decimal price)
        {
            return quantity * price;
        }
    }
}
