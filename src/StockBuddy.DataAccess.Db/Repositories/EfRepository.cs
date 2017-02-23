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
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Mapping;

namespace StockBuddy.DataAccess.Db.Repositories
{
    internal class EfRepository<T> : IRepository<T> where T : Entity
    {
        private StockBuddyDbContext _dbContext;
        private DbContextTransaction _transaction;
        private IDbSet<T> _dbSet;

        public EfRepository()
        {
        }

        private void Initialize(StockBuddyDbContext dbContext, DbContextTransaction transaction)
        {
            Guard.AgainstNull(() => dbContext);

            _dbContext = dbContext;
            _transaction = transaction;
            _dbSet = Table<T>();
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

            return dbSet.SingleOrDefault(p => p.ID == id);
        }

        public IEnumerable<T> GetByIds(int[] ids, params Expression<Func<T, object>>[] includePaths)
        {
            Guard.AgainstNull(() => ids);

            if (ids.Length == 0)
                return Enumerable.Empty<T>();

            IQueryable<T> dbSet = _dbSet;

            foreach (var includePath in includePaths)
                dbSet = dbSet.Include(includePath);

            return dbSet.Where(p => ids.Contains(p.ID)).ToArray();
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

        public void BulkInsert(IEnumerable<T> entities, int batchSize = 0)
        {
            if (entities == null || !entities.Any())
                return;

            SqlConnection connection = _dbContext.Database.Connection as SqlConnection;

            if (connection == null)
                throw new InvalidOperationException("SqlConnection required for bulk insert");

            SqlBulkCopy sqlBulkCopy = null;

            if (_transaction != null)
            {
                SqlTransaction sqlTransaction = _transaction.UnderlyingTransaction as SqlTransaction;

                if (sqlTransaction == null)
                    throw new InvalidOperationException("SqlTransaction required for bulk insert");

                // Explicit transaction is used. The bulk copy will not commit anything.
                sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, sqlTransaction);
            }
            else
            {
                // No explicit transaction. Each batch of the bulk copy will be committed individually.
                // If batchsize is set and an error occurs midway, the successfull batches will not be rolled back.
                sqlBulkCopy = new SqlBulkCopy(connection);
            }

            bool hasOpenedConnection = false;

            try
            {
                using (sqlBulkCopy)
                {
                    sqlBulkCopy.BatchSize = batchSize;
                    sqlBulkCopy.DestinationTableName = _dbContext.GetTableName(typeof(T));

                    var entityType = typeof(T);
                    var dataTable = new DataTable();
                    
                    var entityProperties =
                        _dbContext.GetColumnMappings(typeof(T))
                        .Where(p => !string.Equals(p.Item1, "Id", StringComparison.OrdinalIgnoreCase))
                        .Select(p =>
                        {
                            string propertyName = p.Item1;
                            string columnName = p.Item2;
                            Type dataType = p.Item3;
                        
                            sqlBulkCopy.ColumnMappings.Add(columnName, columnName);
                            dataTable.Columns.Add(columnName, dataType);

                            return new
                            {
                                ColumnName = columnName,
                                PropertyInfo = entityType.GetProperty(propertyName)
                            };
                        })
                        .ToArray();

                    foreach (T entity in entities)
                    {
                        var row = dataTable.NewRow();

                        foreach (var entityProperty in entityProperties)
                        {
                            var value = entityProperty.PropertyInfo.GetValue(entity);
                            row[entityProperty.ColumnName] = value;
                        }

                        dataTable.Rows.Add(row);
                    }

                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                        hasOpenedConnection = true;
                    }

                    sqlBulkCopy.WriteToServer(dataTable);
                }
            }
            finally
            {
                // If we opened the connection, also close it again.
                if (hasOpenedConnection && connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        protected IDbSet<TEntity> Table<TEntity>() where TEntity : class
        {
            return _dbContext.Set<TEntity>();
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
