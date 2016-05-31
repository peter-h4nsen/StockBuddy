using System;
using System.Collections.Generic;
using System.Linq;
using StockBuddy.Client.Shared.ViewModels;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.DTO;
using StockBuddy.Client.Shared.DomainGateways.Impl;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.DomainGateways.Mapping
{
    public sealed class ModelToViewModelMapper
    {
        private readonly GatewayCache _cache;

        public ModelToViewModelMapper(GatewayCache cache)
        {
            Guard.AgainstNull(() => cache);

            _cache = cache;
        }

        public IEnumerable<DepositViewModel> MapToDepositViewModels(IEnumerable<DepositInfoDTO> depositInfoDtos)
        {
            return depositInfoDtos.Select(MapToDepositViewModel);
        }

        public IEnumerable<StockViewModel> MapToStockViewModels(IEnumerable<Stock> stocks)
        {
            var stockVms = stocks.Select(MapToStockViewModel).ToArray();

            _cache.AddStocks(stockVms);

            // Map stocksplits after stocks have been added to the cache,
            // because the stocksplit mapping accesses the cache.
            foreach (var stock in stocks)
            {
                var stockVm = _cache.GetStock(stock.Id);

                foreach (var stockSplit in stock.GetStockSplits())
                {
                    stockVm.StockSplits.Add(GetStockSplitViewModel(stockSplit));
                }
            }
                
            return stockVms;
        }

        public DepositViewModel MapToDepositViewModel(DepositInfoDTO depositInfoDTO)
        {
            var depositVm = GetDepositViewModel(depositInfoDTO.Deposit);
            depositVm.SellableStockIds = depositInfoDTO.SellableStockIds;

            if (depositInfoDTO.Deposit.Trades != null)
            {
                foreach (var trade in depositInfoDTO.Deposit.Trades)
                {
                    var tradeVm = GetTradeViewModel(trade);
                    tradeVm.Deposit = depositVm;

                    if (trade.Stock != null)
                        tradeVm.Stock = _cache.GetStock(trade.StockId);
                        //tradeVm.Stock = GetStockViewModel(trade.Stock);

                    depositVm.Trades.Add(tradeVm);
                }
            }

            if (depositInfoDTO.Deposit.Dividends != null)
            {
                foreach (var dividend in depositInfoDTO.Deposit.Dividends)
                {
                    var dividendVm = GetDividendViewModel(dividend);
                    dividendVm.Deposit = depositVm;
                    depositVm.Dividends.Add(dividendVm);

                    //if (dividend.GeneralMeeting != null)
                    //    dividendVm.GeneralMeeting = GetGeneralMeetingViewModel(dividend.GeneralMeeting);
                }
            }

            return depositVm;
        }

        public StockViewModel MapToStockViewModel(Stock stock)
        {
            var stockVm = GetStockViewModel(stock);

            if (stock.Trades != null)
            {
                foreach (var trade in stock.Trades)
                {
                    var tradeVm = GetTradeViewModel(trade);
                    tradeVm.Stock = stockVm;

                    if (trade.Deposit != null)
                        tradeVm.Deposit = GetDepositViewModel(trade.Deposit);

                    stockVm.Trades.Add(tradeVm);
                }
            }

            return stockVm;
        }

        public IEnumerable<StockSplitViewModel> MapToStockSplitViewModels(IEnumerable<StockSplit> stockSplits)
        {
            return stockSplits.Select(GetStockSplitViewModel);
        }

        public IEnumerable<GeneralMeetingViewModel> MapToGeneralMeetingViewModels(IEnumerable<GeneralMeeting> generalMeetings)
        {
            return generalMeetings.Select(GetGeneralMeetingViewModel);
        }

        public IEnumerable<DividendViewModel> MapToDividendViewModels(IEnumerable<Dividend> dividends)
        {
            return dividends.Select(GetDividendViewModel);
        }

        private DepositViewModel GetDepositViewModel(Deposit deposit)
        {
            return new DepositViewModel
            {
                Id = deposit.Id,
                Description = deposit.Description,
                IdentityNumber = deposit.IdentityNumber,
                DepositType = deposit.DepositType
            };
        }

        private TradeViewModel GetTradeViewModel(Trade trade)
        {
            return new TradeViewModel
            {
                Id = trade.Id,
                IsBuy = trade.IsBuy,
                IsSale = !trade.IsBuy,
                Quantity = trade.Quantity,
                Price = trade.Price,
                Commission = trade.Commission,
                TradeDate = trade.TradeDate
            };
        }

        private StockViewModel GetStockViewModel(Stock stock)
        {
            return new StockViewModel
            {
                Id = stock.Id,
                Name = stock.Name,
                Symbol = stock.Symbol,
                Isin = stock.Isin,
                StockType = stock.StockType,
                IsActive = stock.IsActive,
            };
        }

        private StockSplitViewModel GetStockSplitViewModel(StockSplit stockSplit)
        {
            return new StockSplitViewModel
            {
                Id = stockSplit.Id,
                Date = stockSplit.Date,
                OldStock = _cache.GetStock(stockSplit.OldStockID),
                NewStock = _cache.GetStock(stockSplit.NewStockID),
                RatioFrom = stockSplit.RatioFrom,
                RatioTo = stockSplit.RatioTo
            };
        }

        private GeneralMeetingViewModel GetGeneralMeetingViewModel(GeneralMeeting generalMeeting)
        {
            return new GeneralMeetingViewModel
            {
                Id = generalMeeting.Id,
                MeetingDate = generalMeeting.MeetingDate,
                Stock = _cache.GetStock(generalMeeting.StockID),
                DividendRate = generalMeeting.DividendRate
            };
        }

        private DividendViewModel GetDividendViewModel(Dividend dividend)
        {
            var vm = new DividendViewModel
            {
                Id = dividend.Id,
                Quantity = dividend.Quantity,
                DividendPayment = dividend.DividendPayment,
                IsCreated = dividend.IsCreated,
                IsDifferent = dividend.IsDifferent
            };

            if (dividend.GeneralMeeting != null)
                vm.GeneralMeeting = GetGeneralMeetingViewModel(dividend.GeneralMeeting);

            return vm;
        }
    }
}
