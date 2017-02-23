using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Domain.Repositories
{
    public interface IRepository<T> where T : Entity
    {
        IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includePaths);

        T GetById(int id, params Expression<Func<T, object>>[] includePaths);

        IEnumerable<T> GetByIds(int[] ids, params Expression<Func<T, object>>[] includePaths);

        void Add(T entity);

        void UpdateAll(T entity);

        void Delete(int id);
        void Delete(T entity);
        void BatchDelete(Expression<Func<T, bool>> filter);

        void BulkInsert(IEnumerable<T> entities, int batchSize = 0);
    }
}
