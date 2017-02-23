using System;
using StockBuddy.DataAccess.Db.DbContexts;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Repositories;
using System.Linq;
using System.Collections.Generic;

namespace StockBuddy.DataAccess.Db.Repositories
{
    internal sealed class EfStockRepository : EfRepository<Stock>, IStockRepository
    {
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
            return Table<Trade>().Any(p => p.StockID == stockId);
        }

        //TODO: Refactor to C# 7 tuple type
        public Tuple<int, string, DateTime?>[] GetStocksWithLastInfoDate()
        {
            var result = (
                from stock in Table<Stock>()
                where stock.IsActive
                join stockInfo in Table<HistoricalStockInfo>()
                    on stock.ID equals stockInfo.StockID into grp
                select new
                {
                    StockID = stock.ID,
                    Symbol = stock.Symbol,
                    LatestStockInfoDate = grp.Max(p => (DateTime?)p.Date)
                })
                .AsEnumerable()
                .Select(p => Tuple.Create
                (
                    p.StockID, 
                    p.Symbol, 
                    p.LatestStockInfoDate)
                )
                .ToArray();

            return result;
        }
    }
}
