using System;
using StockBuddy.DataAccess.Db.DbContexts;
using StockBuddy.DataAccess.Db.Factories;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Repositories;
using StockBuddy.Shared.Utilities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace StockBuddy.DataAccess.Db.Repositories
{
    internal sealed class EfUnitOfWork : IUnitOfWork
    {
        private readonly EfRepositoryFactory _repositoryFactory;
        private readonly IDictionary<Type, object> _repositoryCache;
        private readonly StockSplit[] _stockSplits;

        private readonly StockBuddyDbContext _dbContext;
        private readonly DbContextTransaction _transaction;
        
        public EfUnitOfWork(EfRepositoryFactory repositoryFactory, string connectionString, 
            bool useExplicitTransaction, StockSplit[] stockSplits)
        {
            Guard.AgainstNull(() => repositoryFactory, () => connectionString);
            
            _repositoryFactory = repositoryFactory;
            _stockSplits = stockSplits;
            _repositoryCache = new Dictionary<Type, object>();
            _dbContext = new StockBuddyDbContext(connectionString);

            if (useExplicitTransaction)
            {
                _transaction = _dbContext.Database.BeginTransaction();
            }

            _dbContext.ObjectContext.ObjectMaterialized += OnObjectMaterialized;
            _dbContext.Database.Log = (s) => Debug.WriteLine(s);
        }

        public IRepository<TEntity> RepoOf<TEntity>() where TEntity : Entity => 
            CreateRepository(_repositoryFactory.CreateDefaultRepository<TEntity>);
        
        public TRepo Repo<TRepo>() where TRepo : class =>
            CreateRepository(_repositoryFactory.CreateSpecialRepository<TRepo>);
        
        public void SaveChanges()
        {
            ThrowOnInvalidState(false);
            _dbContext.SaveChanges();
        }

        public void CommitTransaction()
        {
            ThrowOnInvalidState(true);
            _transaction.Commit();
        }

        public void RollbackTransaction()
        {
            ThrowOnInvalidState(true);
            _transaction.Rollback();
        }

        private void ThrowOnInvalidState(bool checkExplicitTransaction)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (checkExplicitTransaction && _transaction == null)
                throw new InvalidOperationException("Explicit transaction not enabled.");
        }

        #region StockSplits (Should probably not be in this class)
        private void OnObjectMaterialized(object sender, ObjectMaterializedEventArgs e)
        {
            // When EF initializes a Stock entity, stocksplits are added to it.
            var stock = e.Entity as Stock;

            if (stock != null && _stockSplits != null)
            {
                stock.SetStockSplits(GetAllStockSplits(stock.ID, _stockSplits));
            }
        }

        private List<StockSplit> GetAllStockSplits(int stockId, StockSplit[] stockSplitsLookup)
        {
            var resultList = new List<StockSplit>();
            FindRecursive(stockId, stockSplitsLookup, resultList);
            return resultList.OrderBy(p => p.Date).ToList();
        }

        private void FindRecursive(int stockId, StockSplit[] stockSplitsLookup, List<StockSplit> resultList)
        {
            // Find all stocksplits which influence the current stock.
            // That is the case when the stock is used as a old stock in a stocksplit.
            var stockSplits = stockSplitsLookup.Where(p => p.OldStockID == stockId);

            // A stock can be splitted into itself multiple times, so a loop is needed.
            foreach (var stockSplit in stockSplits)
            {
                resultList.Add(stockSplit);

                // Make recursive call for the new stock of the split, but only if it's not splitted into itself.
                if (stockSplit.OldStockID != stockSplit.NewStockID)
                {
                    FindRecursive(stockSplit.NewStockID, stockSplitsLookup, resultList);
                }
            }
        }
        #endregion

        #region Repository creation and caching
        private TRepo CreateRepository<TRepo>(
                Func<StockBuddyDbContext, DbContextTransaction, TRepo> factory) where TRepo : class
        {
            Guard.AgainstNull(() => factory);

            TRepo repository = GetCachedRepository<TRepo>();

            if (repository == null)
            {
                repository = factory(_dbContext, _transaction);
                _repositoryCache[typeof(TRepo)] = repository;
            }

            return repository;
        }

        private T GetCachedRepository<T>() where T : class
        {
            object repository;
            _repositoryCache.TryGetValue(typeof(T), out repository);
            return (T)repository;
        }

        #endregion

        #region Disposal

        private bool _isDisposed = false;

        public void Dispose()
        {
            Dispose(true);

            // We don't have a finalizer in this case, but just to be safe in case one is added later.
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            // It's safe to call this method multiple times, but only the first call does any work.
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    // Called by dispose method via using-statement or similar. Clean up managed resources here.

                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                    }
                        
                    _dbContext.Dispose();
                }

                // If 'isDisposing' is false, the method was called by the finalizer during garbage collection,
                // and we only clean up unmanaged resources. Managed resources will be cleaned up by their own finalizers.
                // (We don't have any unmanaged resources in this case though).

                _isDisposed = true;
            }
        }
        #endregion
    }
}
