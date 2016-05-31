using System;
using StockBuddy.Client.Shared.ViewModels;
using StockBuddy.Domain.DTO;

namespace StockBuddy.Client.Shared.DomainGateways.Contracts
{
    public interface ITradeGateway
    {
        void Create(TradeViewModel tradeVm);
        //int[] GetSellableStocks(DepositViewModel depositVm);
        TradeInfoDto GetTradeInfo(TradeViewModel trade, DepositViewModel deposit);
    }
}
