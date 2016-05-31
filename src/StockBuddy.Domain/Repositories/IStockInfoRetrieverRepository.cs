using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Domain.Repositories
{
    public interface IStockInfoRetrieverRepository
    {
        Task<IEnumerable<HistoricalStockInfo>> GetHistoricalStockInfo(
            string symbol, DateTime fromDate, DateTime toDate);
    }
}
