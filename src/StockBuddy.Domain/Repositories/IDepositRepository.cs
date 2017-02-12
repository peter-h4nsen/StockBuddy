using System;
using StockBuddy.Domain.Entities;
using System.Collections.Generic;

namespace StockBuddy.Domain.Repositories
{
    public interface IDepositRepository : IRepository<Deposit>
    {
        IEnumerable<Deposit> GetAllWithIncludes();
        Deposit GetByIdWithIncludes(int id);
        void Update(Deposit deposit);
    }
}
