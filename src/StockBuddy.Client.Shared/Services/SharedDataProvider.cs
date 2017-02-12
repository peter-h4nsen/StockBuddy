using System;
using System.Collections.ObjectModel;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Client.Shared.ViewModels;
using StockBuddy.Shared.Utilities;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace StockBuddy.Client.Shared.Services
{
    public sealed class SharedDataProvider : ISharedDataProvider
    {
        private readonly IStockGateway _stockGateway;
        private readonly IDepositGateway _depositGateway;

        public SharedDataProvider(IStockGateway stockGateway, IDepositGateway depositGateway)
        {
            Guard.AgainstNull(() => stockGateway, () => depositGateway);

            _stockGateway = stockGateway;
            _depositGateway = depositGateway;

            Stocks = new ObservableCollection<StockViewModel>();
            Deposits = new ObservableCollection<DepositViewModel>();
            GeneralMeetings = new ObservableCollection<GeneralMeetingViewModel>();
            GeneralMeetingYears = new ObservableCollection<KeyValuePair<string, int>>();

            Update();
        }

        public ObservableCollection<StockViewModel> Stocks { get; }
        public ObservableCollection<DepositViewModel> Deposits { get; }
        public ObservableCollection<GeneralMeetingViewModel> GeneralMeetings { get; }
        public ObservableCollection<KeyValuePair<string, int>> GeneralMeetingYears { get; }

        public void Update()
        {
            Stocks.Clear();

            foreach (var stock in _stockGateway.GetAll())
                Stocks.Add(stock);

            Deposits.Clear();

            foreach (var deposit in _depositGateway.GetAll())
                Deposits.Add(deposit);

            GeneralMeetings.Clear();

            foreach (var generalMeeting in _stockGateway.GetAllGeneralMeetings())
                GeneralMeetings.Add(generalMeeting);

            UpdateGeneralMeetingYears();

        }

        public void DeleteStock(StockViewModel stock)
        {
            _stockGateway.Delete(stock.Id);
            Stocks.Remove(stock);
        }

        public void AddStock(StockViewModel stock)
        {
            _stockGateway.Create(stock);
            Stocks.Add(stock);
        }

        public void RefreshDepositTradeAdded(DepositViewModel depositVm, int tradeId)
        {
            var refreshedDepositVm = _depositGateway.Refresh(depositVm.Id);
            var refreshedTradeVm = refreshedDepositVm.Trades.Single(p => p.Id == tradeId);

            depositVm.SellableStockIds = refreshedDepositVm.SellableStockIds;
            depositVm.Trades.Add(refreshedTradeVm);

            var refreshedPosition = refreshedDepositVm.StockPositions.Single(p => p.Stock.Id == refreshedTradeVm.Stock.Id);
            var existingPosition = depositVm.StockPositions.SingleOrDefault(p => p.Stock.Id == refreshedTradeVm.Stock.Id);

            if (existingPosition != null)
                existingPosition.Quantity = refreshedPosition.Quantity;
            else
                depositVm.StockPositions.Add(refreshedPosition);

            // Add new trade to the collection of stocks.
            var stock = Stocks.Single(p => p.Id == refreshedTradeVm.Stock.Id);
            stock.Trades.Add(refreshedTradeVm);
        }

        public void DeleteDeposit(DepositViewModel deposit)
        {
            _depositGateway.Delete(deposit.Id);
            Deposits.Remove(deposit);
        }

        public void AddDeposit(DepositViewModel deposit)
        {
            _depositGateway.Create(deposit);
            Deposits.Add(deposit);
        }

        public void DeleteGeneralMeeting(GeneralMeetingViewModel generalMeeting)
        {
            _stockGateway.DeleteGeneralMeeting(generalMeeting.Id);
            GeneralMeetings.Remove(generalMeeting);
            UpdateGeneralMeetingYears();
        }

        public void AddGeneralMeeting(GeneralMeetingViewModel generalMeeting)
        {
            _stockGateway.CreateGeneralMeeting(generalMeeting);
            GeneralMeetings.Add(generalMeeting);
            UpdateGeneralMeetingYears();
        }

        public void UpdateGeneralMeeting(GeneralMeetingViewModel generalMeeting)
        {
            _stockGateway.UpdateGeneralMeeting(generalMeeting);
            UpdateGeneralMeetingYears();
        }

        public void UpdateGeneralMeetingYears()
        {
            var yearValues = 
                GeneralMeetings.Select(p => p.MeetingDate.Value.Year)
                .Distinct()
                .OrderByDescending(year => year)
                .Select(year => new KeyValuePair<string, int>($"År {year.ToString("G")}", year));

            GeneralMeetingYears.Clear();

            foreach (var yearValue in yearValues)
                GeneralMeetingYears.Add(yearValue);
        }
    }
}
