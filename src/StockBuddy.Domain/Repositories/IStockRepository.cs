using System;
using StockBuddy.Domain.Entities;
using System.Collections.Generic;

namespace StockBuddy.Domain.Repositories
{
    public interface IStockRepository : IRepository<Stock>
    {
        void Update(Stock stock);
        bool IsStockReferenced(int stockId);

        Tuple<int, string, DateTime?>[] GetStocksWithLastInfoDate();
    }
}
