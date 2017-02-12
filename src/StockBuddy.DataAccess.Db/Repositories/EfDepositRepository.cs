using System;
using System.Collections.Generic;
using System.Linq;
using StockBuddy.DataAccess.Db.DbContexts;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Repositories;
using System.Linq.Expressions;

namespace StockBuddy.DataAccess.Db.Repositories
{
    public sealed class EfDepositRepository : EfRepository<Deposit>, IDepositRepository
    {
        private Expression<Func<Deposit, object>>[] includePaths =
        {
            p => p.Trades.Select(t => t.Stock),
            p => p.Dividends.Select(o => o.GeneralMeeting.Stock)
        };

        public EfDepositRepository(StockBuddyDbContext dbContext)
            : base(dbContext)
        {  
        }

        public IEnumerable<Deposit> GetAllWithIncludes() => GetAll(includePaths);

        public Deposit GetByIdWithIncludes(int id) => GetById(id, includePaths);
        
        public void Update(Deposit deposit)
        {
            var entry = GetAttachedEntry(deposit);
            entry.Property(p => p.Description).IsModified = true;
            entry.Property(p => p.IdentityNumber).IsModified = true;
            entry.Property(p => p.DepositType).IsModified = true;
        }
    }
}
