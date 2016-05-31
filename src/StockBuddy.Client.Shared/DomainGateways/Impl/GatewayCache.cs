using StockBuddy.Client.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockBuddy.Client.Shared.DomainGateways.Impl
{
    public sealed class GatewayCache
    {
        private IDictionary<int, StockViewModel> _stocksCache;

        public void AddStocks(IEnumerable<StockViewModel> stocks)
        {
            _stocksCache = stocks.ToDictionary(p => p.Id);
        }

        public StockViewModel GetStock(int id)
        {
            StockViewModel vm;

            if (!_stocksCache.TryGetValue(id, out vm))
                throw new InvalidOperationException($"Stock not found in cache. Id: {id}");

            return vm;
        }
    }
}
