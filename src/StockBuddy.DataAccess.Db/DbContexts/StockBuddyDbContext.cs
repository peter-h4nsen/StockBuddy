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
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Mapping;

namespace StockBuddy.DataAccess.Db.DbContexts
{
    [DbConfigurationType(typeof(DefaultDbConfig))]
    internal sealed class StockBuddyDbContext : DbContext
    {
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

        public string GetTableName(Type type)
        {
            var tableEntitySet = GetMappingFragment(type).StoreEntitySet;

            var tableName =
                (string)tableEntitySet.MetadataProperties["Table"].Value ??
                tableEntitySet.Name;

            return tableName;
        }

        public Tuple<string, string, Type>[] GetColumnMappings(Type type)
        {
            var propertyMappings = GetMappingFragment(type).PropertyMappings;

            var columnMappings =
                propertyMappings.OfType<ScalarPropertyMapping>()
                .Select(p => Tuple.Create
                (
                    p.Property.Name,
                    p.Column.Name,
                    p.Column.PrimitiveType.ClrEquivalentType)
                )
                .ToArray();

            return columnMappings;
        }

        private MappingFragment GetMappingFragment(Type type)
        {
            var metaData = ObjectContext.MetadataWorkspace;

            var objectItemCollection = ((ObjectItemCollection)metaData.GetItemCollection(DataSpace.OSpace));

            var entityType =
                metaData.GetItems<EntityType>(DataSpace.OSpace)
                .Single(e => objectItemCollection.GetClrType(e) == type);

            var entitySet =
                metaData.GetItems<EntityContainer>(DataSpace.CSpace).Single()
                .EntitySets.Single(s => s.ElementType.Name == entityType.Name);

            var mapping =
                metaData.GetItems<EntityContainerMapping>(DataSpace.CSSpace).Single()
                .EntitySetMappings.Single(s => s.EntitySet == entitySet);

            var mappingFragment =
                mapping.EntityTypeMappings.Single()
                .Fragments.Single();

            return mappingFragment;
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
