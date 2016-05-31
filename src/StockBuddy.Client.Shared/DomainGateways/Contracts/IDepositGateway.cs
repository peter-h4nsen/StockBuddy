using System;
using System.Collections.Generic;
using StockBuddy.Client.Shared.ViewModels;
using StockBuddy.Domain.DTO.YearlyReport;

namespace StockBuddy.Client.Shared.DomainGateways.Contracts
{
    public interface IDepositGateway
    {
        void Create(DepositViewModel depositVm);
        void Update(DepositViewModel depositVm);
        void Delete(int depositId);

        IEnumerable<DepositViewModel> GetAll();
        DepositViewModel Refresh(DepositViewModel depositVm, TradeViewModel tradeVm);

        IEnumerable<DividendViewModel> CalculateDividends(
            int year, DepositViewModel depositVm, IEnumerable<GeneralMeetingViewModel> generalMeetingVms);

        void CreateDividend(DividendViewModel dividendVm);
        void DeleteDividend(int dividendId);

        YearlyReportDTO GetYearlyReport(int year, bool isMarried, DepositViewModel depositVm);
    }
}
