using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.DTO;

namespace StockBuddy.Domain.Repositories
{
    public interface IStockInfoRetrieverRepository
    {
        Task<HistoricalStockInfoResult> GetHistoricalStockInfo(
            string symbol, DateTime fromDate, DateTime toDate);
    }
}
