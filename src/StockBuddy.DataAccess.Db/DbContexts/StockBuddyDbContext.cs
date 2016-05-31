using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using StockBuddy.Domain.Entities;
using StockBuddy.DataAccess.Db.Configuration;
using System.Linq;
using System.Data.Entity.ModelConfiguration;
using System.Reflection;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;

namespace StockBuddy.DataAccess.Db.DbContexts
{
    [DbConfigurationType(typeof(DefaultDbConfig))]
    public sealed class StockBuddyDbContext : DbContext
    {
        public IDbSet<Trade> Trades { get { return Set<Trade>(); } }

        static StockBuddyDbContext()
        {
            Database.SetInitializer<StockBuddyDbContext>(null);
        }

        public StockBuddyDbContext(string connectionString)
            : base(connectionString)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Entity<HistoricalStockInfo>().ToTable("HistoricalStockInfo");

            RegisterEntities(modelBuilder);
        }

        public ObjectContext ObjectContext
        {
            get { return ((IObjectContextAdapter)this).ObjectContext; }
        }

        /// <summary>
        /// Finds all entities and makes sure the are registered as part of the EF model.
        /// </summary>
        private void RegisterEntities(DbModelBuilder modelBuilder)
        {
            var entityTypes =
                typeof(Entity).Assembly.GetTypes()
                .Where(p => p.IsSubclassOf(typeof(Entity)));

            // Gets the generic method DbModelBuilder.Entity and calls it for each entity type.
            Func<EntityTypeConfiguration<object>> entityMethodDelegate = modelBuilder.Entity<object>;
            MethodInfo genericMethodDefinition = entityMethodDelegate.Method.GetGenericMethodDefinition();

            foreach (var entityType in entityTypes)
            {
                MethodInfo method = genericMethodDefinition.MakeGenericMethod(entityType);
                method.Invoke(modelBuilder, null);
            }
        }
    }
}
