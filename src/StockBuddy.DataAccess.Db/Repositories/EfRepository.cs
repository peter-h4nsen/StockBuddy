using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using StockBuddy.DataAccess.Db.DbContexts;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Repositories;
using StockBuddy.Shared.Utilities;
using EntityFramework.Extensions;
using System.Reflection;

namespace StockBuddy.DataAccess.Db.Repositories
{
    public class EfRepository<T> : IRepository<T> where T : Entity
    {
        protected readonly StockBuddyDbContext _dbContext;
        private readonly IDbSet<T> _dbSet;

        public EfRepository(StockBuddyDbContext dbContext)
        {
            Guard.AgainstNull(() => dbContext);

            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includePaths)
        {
            IQueryable<T> dbSet = _dbSet;

            foreach (var includePath in includePaths)
                dbSet = dbSet.Include(includePath);

            return dbSet.ToArray();
        }

        public T GetById(int id, params Expression<Func<T, object>>[] includePaths)
        {
            IQueryable<T> dbSet = _dbSet;

            foreach (var includePath in includePaths)
                dbSet = dbSet.Include(includePath);

            return dbSet.SingleOrDefault(p => p.Id == id);
        }

        public IEnumerable<T> GetByIds(int[] ids, params Expression<Func<T, object>>[] includePaths)
        {
            Guard.AgainstNull(() => ids);

            if (ids.Length == 0)
                return Enumerable.Empty<T>();

            IQueryable<T> dbSet = _dbSet;

            foreach (var includePath in includePaths)
                dbSet = dbSet.Include(includePath);

            return dbSet.Where(p => ids.Contains(p.Id)).ToArray();
        }

        public void Add(T entity)
        {
            Guard.AgainstNull(() => entity);

            var entry = GetAttachedEntry(entity);
            entry.State = EntityState.Added;
        }

        public void UpdateAll(T entity)
        {
            Guard.AgainstNull(() => entity);

            var entry = GetAttachedEntry(entity);
            entry.State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            var entityType = typeof(T);
            T entity = (T)Activator.CreateInstance(entityType, true);
            var idProperty = entityType.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
            idProperty.SetValue(entity, id);
            Delete(entity);
        }

        public void Delete(T entity)
        {
            Guard.AgainstNull(() => entity);

            var entry = GetAttachedEntry(entity);
            entry.State = EntityState.Deleted;
        }

        public void BatchDelete(Expression<Func<T, bool>> filter)
        {
            _dbSet.Where(filter).Delete();
        }

        protected DbEntityEntry<T> GetAttachedEntry(T entity)
        {
            var entry = _dbContext.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }

            return entry;
        }
    }
}
