using System;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        void SaveChanges();
        void CommitTransaction();
        void RollbackTransaction();

        TRepo Repo<TRepo>() where TRepo : class;
        IRepository<TEntity> RepoOf<TEntity>() where TEntity : Entity;
    }
}
