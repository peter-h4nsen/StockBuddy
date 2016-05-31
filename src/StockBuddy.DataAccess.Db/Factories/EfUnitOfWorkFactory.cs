using System;
using StockBuddy.DataAccess.Db.Repositories;
using StockBuddy.Domain.Factories;
using StockBuddy.Domain.Repositories;
using StockBuddy.Shared.Utilities;
using StockBuddy.Domain.Entities;
using System.Linq;

namespace StockBuddy.DataAccess.Db.Factories
{
    public sealed class EfUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly EfRepositoryFactory _repositoryFactory;
        private readonly string _connectionString;
        private StockSplit[] _stockSplits;

        public EfUnitOfWorkFactory(EfRepositoryFactory repositoryFactory, string connectionString)
        {
            Guard.AgainstNull(() => repositoryFactory, () => connectionString);

            _repositoryFactory = repositoryFactory;
            _connectionString = connectionString;

            // A unit of work will be responsible for adding stocksplits to stocks,
            // so stocksplits are cached in the factory and send to each new unit of work.
            GetStockSplits();
        }

        private void GetStockSplits()
        {
            using (var uow = Create())
            {
                _stockSplits = uow.RepoOf<StockSplit>().GetAll(p => p.OldStock, p => p.NewStock).ToArray();
            }
        }

        public IUnitOfWork Create(bool useExplicitTransaction = false)
        {
            return new EfUnitOfWork(_repositoryFactory, _connectionString, useExplicitTransaction, _stockSplits);
        }
    }
}
