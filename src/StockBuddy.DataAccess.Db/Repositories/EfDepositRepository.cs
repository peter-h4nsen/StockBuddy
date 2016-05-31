using System;
using System.Linq;
using StockBuddy.DataAccess.Db.DbContexts;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Repositories;

namespace StockBuddy.DataAccess.Db.Repositories
{
    public sealed class EfDepositRepository : EfRepository<Deposit>, IDepositRepository
    {
        public EfDepositRepository(StockBuddyDbContext dbContext)
            : base(dbContext)
        {
        }

        public void Update(Deposit deposit)
        {
            var entry = GetAttachedEntry(deposit);
            entry.Property(p => p.Description).IsModified = true;
            entry.Property(p => p.IdentityNumber).IsModified = true;
            entry.Property(p => p.DepositType).IsModified = true;
        }

        public Trade[] GetTrades(int depositId, int stockId)
        {
            return (
                from trade in _dbContext.Trades
                where trade.DepositId == depositId &&
                      trade.StockId == stockId
                select trade).ToArray();
        }
    }
}
