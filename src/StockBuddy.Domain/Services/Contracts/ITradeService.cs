using System;
using System.Collections.Generic;
using StockBuddy.Domain.DTO;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Domain.Services.Contracts
{
    public interface ITradeService
    {
        Trade CreateTrade(Trade trade);
        //IEnumerable<int> GetSellableStocks(Deposit deposit);
        
        TradeInfoDto CalculateTradeInfo(bool isBuy, decimal tradePrice, int quantity, int stockId, int depositId);
    }
}
