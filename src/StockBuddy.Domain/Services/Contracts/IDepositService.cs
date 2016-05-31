using System;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.DTO;
using System.Collections.Generic;
using StockBuddy.Domain.DTO.YearlyReport;

namespace StockBuddy.Domain.Services.Contracts
{
    public interface IDepositService
    {
        Deposit CreateDeposit(Deposit deposit);
        void UpdateDeposit(Deposit deposit);
        void DeleteDeposit(int depositId);
        
        IEnumerable<DepositInfoDTO> GetAll();
        DepositInfoDTO Refresh(Deposit deposit, Trade trade);

        YearlyReportDTO GetYearlyReport(int year, bool isMarried, Deposit deposit);
    }
}
