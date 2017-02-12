using System;
using System.Collections.Generic;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Domain.Services.Contracts
{
    public interface IDividendService
    {
        IEnumerable<Dividend> CalculateDividends(int year, int depositId);

        Dividend CreateDividend(Dividend dividend);
        void DeleteDividend(int dividendId);
    }
}
