using System;
using StockBuddy.DataAccess.Db.DbContexts;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Repositories;
using System.Linq;

namespace StockBuddy.DataAccess.Db.Repositories
{
    internal sealed class EfStockRepository : EfRepository<Stock>, IStockRepository
    {
        public EfStockRepository(StockBuddyDbContext dbContext)
            : base(dbContext)
        {
        }

        public void Update(Stock stock)
        {
            var entry = GetAttachedEntry(stock);
            entry.Property(p => p.Name).IsModified = true;
            entry.Property(p => p.Symbol).IsModified = true;
            entry.Property(p => p.Isin).IsModified = true;
            entry.Property(p => p.StockType).IsModified = true;
            entry.Property(p => p.IsActive).IsModified = true;
        }

        public bool IsStockReferenced(int stockId)
        {
            return _dbContext.Trades.Any(p => p.StockId == stockId);
        }
    }
}
