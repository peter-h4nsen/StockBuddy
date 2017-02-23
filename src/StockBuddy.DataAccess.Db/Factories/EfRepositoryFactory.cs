using System;
using System.Linq;
using StockBuddy.DataAccess.Db.DbContexts;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Repositories;
using StockBuddy.Shared.Utilities;
using StockBuddy.DataAccess.Db.Repositories;
using System.Reflection;
using System.Data.Entity;

namespace StockBuddy.DataAccess.Db.Factories
{
    public sealed class EfRepositoryFactory
    {
        private const string RepositoryNamePrefix = "I";
        private const string RepositoryNameSuffix = "Repository";

        private readonly Type[] _repositoryTypes;

        public EfRepositoryFactory()
        {
            _repositoryTypes = (
                from type in typeof(EfRepositoryFactory).Assembly.GetTypes()
                where type.Name.EndsWith(RepositoryNameSuffix)
                select type).ToArray();
        }

        /// <summary>
        /// Genererer en repository instans ud fra type-parameterens navn.
        /// Typen skal være interfacetypen på det repo det skal genereres.
        /// Hvis typen er f.eks. IStockRepository ledes efter typen EfStockRepository og en instans returneres.
        /// </summary>
        internal T CreateSpecialRepository<T>(
                StockBuddyDbContext dbContext, DbContextTransaction transaction) where T : class
        {
            Guard.AgainstNull(() => dbContext);

            if (!typeof(T).IsInterface)
            {
                throw new InvalidOperationException(
                    $"Can't create repository. Type must be an interface.");
            }

            var repoInterfaceName = typeof(T).Name;

            if (!repoInterfaceName.StartsWith(RepositoryNamePrefix))
            {
                throw new InvalidOperationException(
                    $"Can't create repository. Type name must start with '{RepositoryNamePrefix}'. Supplied: {repoInterfaceName}");
            }

            if (!repoInterfaceName.EndsWith(RepositoryNameSuffix))
            {
                throw new InvalidOperationException(
                    $"Can't create repository. Type name must end with '{RepositoryNameSuffix}'. Supplied: {repoInterfaceName}");
            }

            var repoName = repoInterfaceName.ReplaceFirst(RepositoryNamePrefix, "Ef");
            var repoType = _repositoryTypes.SingleOrDefault(p => p.Name == repoName);

            if (repoType == null)
            {
                throw new InvalidOperationException(
                    $"Can't create repository. No type found with name: {repoName}");
            }

            // Vær sikker på at den fundne type implementerer interfacet. Ellers fejler castet længere nede.
            if (!typeof(T).IsAssignableFrom(repoType))
            {
                throw new InvalidOperationException(
                    $"Can't create repository. {repoName} is not a {repoInterfaceName}.");
            }

            T repository = (T)Activator.CreateInstance(repoType);
            SetRepositoryDependencies(repository, dbContext, transaction);
            return repository;
        }

        internal IRepository<T> CreateDefaultRepository<T>(
                StockBuddyDbContext dbContext, DbContextTransaction transaction) where T : Entity
        {
            if (dbContext == null)
                throw new ArgumentNullException("dbContext");

            var repository = new EfRepository<T>();
            SetRepositoryDependencies(repository, dbContext, transaction);
            return repository;
        }

        private void SetRepositoryDependencies(
                object repository, StockBuddyDbContext dbContext, DbContextTransaction transaction)
        {
            var method = 
                GetMethod(repository.GetType()) ?? 
                GetMethod(repository.GetType().BaseType);

            if (method == null)
                throw new InvalidOperationException("Could not set dependencies on repository");

            method.Invoke(repository, new object[] { dbContext, transaction });
        }

        private MethodInfo GetMethod(Type repositoryType)
        {
            return repositoryType.GetMethod(
                "Initialize", 
                BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
