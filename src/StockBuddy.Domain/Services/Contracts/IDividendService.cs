using System;
using System.Collections.Generic;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Domain.Services.Contracts
{
    public interface IDividendService
    {
        IEnumerable<Dividend> CalculateDividends(int year, Deposit deposit, IEnumerable<GeneralMeeting> generalMeetings);

        Dividend CreateDividend(Dividend dividend);
        void DeleteDividend(int dividendId);
    }
}
