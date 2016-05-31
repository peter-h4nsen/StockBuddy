using System;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Domain.Repositories
{
    public interface IStockRepository : IRepository<Stock>
    {
        void Update(Stock stock);
        bool IsStockReferenced(int stockId);
    }
}
