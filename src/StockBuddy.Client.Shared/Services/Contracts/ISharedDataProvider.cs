using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using StockBuddy.Client.Shared.ViewModels;

namespace StockBuddy.Client.Shared.Services.Contracts
{
    public interface ISharedDataProvider
    {
        ObservableCollection<StockViewModel> Stocks { get; }
        ObservableCollection<DepositViewModel> Deposits { get; }
        ObservableCollection<GeneralMeetingViewModel> GeneralMeetings { get; }
        ObservableCollection<KeyValuePair<string, int>> GeneralMeetingYears { get; }

        void Update();

        void DeleteStock(StockViewModel stock);
        void AddStock(StockViewModel stock);

        void DeleteDeposit(DepositViewModel deposit);
        void AddDeposit(DepositViewModel deposit);
        void RefreshDepositTradeAdded(DepositViewModel deposit, int tradeId);

        void DeleteGeneralMeeting(GeneralMeetingViewModel generalMeeting);
        void AddGeneralMeeting(GeneralMeetingViewModel generalMeeting);
        void UpdateGeneralMeeting(GeneralMeetingViewModel generalMeeting);
    }
}
