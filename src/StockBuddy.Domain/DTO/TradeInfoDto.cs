using System;

namespace StockBuddy.Domain.DTO
{
    public struct TradeInfoDto
    {
        public TradeInfoDto(
            int currentStockQuantity,decimal originalTradeValue, decimal currentTradeValue,
            decimal stockValueInDeposit, decimal tradeShareInDeposit, decimal stockShareInDeposit)
        {
            CurrentStockQuantity = currentStockQuantity;
            OriginalTradeValue = originalTradeValue;
            CurrentTradeValue = currentTradeValue;
            StockValueInDeposit = stockValueInDeposit;
            TradeShareInDeposit = tradeShareInDeposit;
            StockShareInDeposit = stockShareInDeposit;
        }

        public int CurrentStockQuantity { get; }
        public decimal OriginalTradeValue { get; }
        public decimal CurrentTradeValue { get; }
        public decimal StockValueInDeposit { get; }
        public decimal TradeShareInDeposit { get; }
        public decimal StockShareInDeposit { get; }
    }
}
