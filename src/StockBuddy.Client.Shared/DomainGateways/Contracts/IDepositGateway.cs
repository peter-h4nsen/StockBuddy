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
        DepositViewModel Refresh(int depositId);

        IEnumerable<DividendViewModel> CalculateDividends(
            int year, DepositViewModel depositVm);

        void CreateDividend(DividendViewModel dividendVm);
        void DeleteDividend(int dividendId);

        YearlyReportDTO GetYearlyReport(int year, bool isMarried, int depositId);
    }
}
