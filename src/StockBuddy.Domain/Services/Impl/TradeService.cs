using System;
using System.Collections.Generic;
using System.Linq;
using StockBuddy.Domain.DTO;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Factories;
using StockBuddy.Domain.Services.Contracts;
using StockBuddy.Domain.Validation;
using StockBuddy.Shared.Utilities;
using StockBuddy.Domain.Repositories;

namespace StockBuddy.Domain.Services.Impl
{
    public sealed class TradeService : ITradeService
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IStockPositionCalculator _stockPositionCalculator;

        public TradeService(IUnitOfWorkFactory uowFactory, IStockPositionCalculator stockPositionCalculator)
        {
            Guard.AgainstNull(() => uowFactory, () => stockPositionCalculator);

            _uowFactory = uowFactory;
            _stockPositionCalculator = stockPositionCalculator;
        }

        public Trade CreateTrade(Trade trade)
        {
            Guard.AgainstNull(() => trade);

            trade.Validate();

            using (var uow = _uowFactory.Create())
            {
                uow.RepoOf<Trade>().Add(trade);
                uow.SaveChanges();
                return trade;
            }
        }

        public TradeInfoDto CalculateTradeInfo(bool isBuy, decimal tradePrice, int quantity, int stockId, Deposit deposit)
        {
            Guard.AgainstNull(() => deposit);

            //TODO: Modtag som parameter når vi har realtime-kurser.
            var realtimePrice = 150m;

            var stockQuantity = 0;
            var depositValue = 0m;
            var originalTradeValue = _stockPositionCalculator.CalculateMarketValue(quantity, tradePrice);
            var currentTradeValue = _stockPositionCalculator.CalculateMarketValue(quantity, realtimePrice);
            var stockValueInDeposit = (isBuy ? currentTradeValue : -currentTradeValue);
            var depositPositions = _stockPositionCalculator.GetStockPositions(deposit).ToArray();

            if (depositPositions.Any())
            {
                //TODO: Use real price when the system supports up-to-date prices.
                var fakePrice = 200;

                var existingPosition = depositPositions.SingleOrDefault(p => p.StockId == stockId);

                if (existingPosition != null)
                {
                    stockQuantity = existingPosition.Quantity;
                    stockValueInDeposit += _stockPositionCalculator.CalculateMarketValue(existingPosition.Quantity, fakePrice); //existingPosition.Value;
                }

                depositValue += depositPositions.Sum(p => _stockPositionCalculator.CalculateMarketValue(p.Quantity, fakePrice));// p.Value);
            }

            // Ved køb skal værdien på den nye handel lægges til den eksisterende værdi fordi vi er interesserede i
            // at finde ud af hvordan depotet vil komme til at se ud efter handlen er lavet.
            if (isBuy)
                depositValue += currentTradeValue;

            var tradeShareInDeposit = 0m;
            var stockShareInDeposit = 0m;

            if (depositValue > 0)
            {
                tradeShareInDeposit = BeregnPctAndel(currentTradeValue, depositValue);
                stockShareInDeposit = BeregnPctAndel(stockValueInDeposit, depositValue);
            }

            return new TradeInfoDto(
                stockQuantity, originalTradeValue, currentTradeValue,
                stockValueInDeposit, tradeShareInDeposit, stockShareInDeposit);
        }

        private decimal BeregnPctAndel(decimal mængde, decimal total)
        {
            return mængde / total * 100;
        }
    }
}
