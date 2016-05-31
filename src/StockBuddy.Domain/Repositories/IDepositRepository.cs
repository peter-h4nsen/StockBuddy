using System;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Domain.Repositories
{
    public interface IDepositRepository : IRepository<Deposit>
    {
        void Update(Deposit deposit);
        Trade[] GetTrades(int depositId, int stockId);
    }
}
