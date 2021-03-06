﻿using System;
using StockBuddy.Domain.Entities;
using StockBuddy.Client.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace StockBuddy.Client.Shared.DomainGateways.Mapping
{
    public sealed class ViewModelToModelMapper
    {
        public Deposit MapToDeposit(DepositViewModel depositVm) => GetDeposit(depositVm);
        public Trade MapToTrade(TradeViewModel tradeVm) => GetTrade(tradeVm);
        public Stock MapToStock(StockViewModel stockVm) => GetStock(stockVm);
        public StockSplit MapToStockSplit(StockSplitViewModel stockSplitVm) => GetStockSplit(stockSplitVm, false);
        public GeneralMeeting MapToGeneralMeeting(GeneralMeetingViewModel generalMeetingVm) => GetGeneralMeeting(generalMeetingVm);
        public Dividend MapToDividend(DividendViewModel dividendVm) => GetDividend(dividendVm);
        
        private Deposit GetDeposit(DepositViewModel depositVm)
        {
            if (depositVm == null)
                return null;

            return new Deposit(
                depositVm.Id,
                depositVm.Description,
                depositVm.IdentityNumber,
                depositVm.DepositType);
        }

        private Trade GetTrade(TradeViewModel tradeVm)
        {
            Stock stock = GetStock(tradeVm.Stock);
            Deposit deposit = GetDeposit(tradeVm.Deposit);

            return new Trade(
                tradeVm.Id,
                tradeVm.IsBuy.Value,
                tradeVm.Quantity.Value,
                tradeVm.Price.Value,
                tradeVm.Commission.Value,
                tradeVm.TradeDate.Value,
                tradeVm.Deposit.Id,
                tradeVm.Stock.Id,
                deposit, 
                stock);
        }

        private Stock GetStock(StockViewModel stockVm, bool addStocksplits = true)
        {
            if (stockVm == null)
                return null;

            var stock = new Stock(
                stockVm.Id,
                stockVm.Name,
                stockVm.Symbol,
                stockVm.Isin,
                stockVm.StockType,
                stockVm.IsActive);

            if (addStocksplits)
            { 
                var stockSplits = stockVm.StockSplits.Select(p => GetStockSplit(p, true)).ToList();
                stock.SetStockSplits(stockSplits);
            }

            return stock;
        }

        private StockSplit GetStockSplit(StockSplitViewModel stockSplitVm, bool mapRelations)
        {
            if (stockSplitVm == null)
                return null;

            return new StockSplit(
                stockSplitVm.Id,
                stockSplitVm.Date.Value,
                stockSplitVm.OldStock.Id,
                stockSplitVm.NewStock.Id,
                stockSplitVm.RatioFrom,
                stockSplitVm.RatioTo,
                mapRelations ? GetStock(stockSplitVm.OldStock, false) : null,
                mapRelations ? GetStock(stockSplitVm.NewStock, false) : null);
        }

        private GeneralMeeting GetGeneralMeeting(GeneralMeetingViewModel generalMeetingVm)
        {
            if (generalMeetingVm == null)
                return null;

            return new GeneralMeeting(
                generalMeetingVm.Id,
                generalMeetingVm.MeetingDate.Value,
                generalMeetingVm.DividendRate,
                generalMeetingVm.Stock.Id,
                GetStock(generalMeetingVm.Stock)
            );
        }

        private Dividend GetDividend(DividendViewModel dividendVm)
        {
            var dividend = new Dividend(
                dividendVm.Id, 
                dividendVm.Quantity, 
                dividendVm.GeneralMeeting.Id, 
                dividendVm.Deposit.Id, 
                GetGeneralMeeting(dividendVm.GeneralMeeting),
                false,
                false
            );

            return dividend;
        }
    }
}
