using System;
using StockBuddy.Client.Shared.DomainGateways.Impl;
using StockBuddy.Client.Shared.DomainGateways.Mapping;
using StockBuddy.DataAccess.Db.Factories;
using StockBuddy.Domain.Services.Impl;
using StockBuddy.Shared.Utilities;
using Autofac;

namespace StockBuddy.Client.Shared.Bootstrapping
{
    public sealed class GatewayModule : Module
    {
        private readonly string _connectionstring;
        private readonly string _providerName;

        public GatewayModule(string connectionstring, string providerName)
        {
            Guard.AgainstNull(() => connectionstring, () => providerName);

            _connectionstring = connectionstring;
            _providerName = providerName;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .InNamespaceOf<DepositGateway>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder
                .RegisterType<ModelToViewModelMapper>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<ViewModelToModelMapper>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<GatewayCache>()
                .AsSelf()
                .SingleInstance();

            //builder
            //    .Register(p => new DbContextFactory(_connectionstring, _providerName))
            //    .AsSelf()
            //    .SingleInstance();

            builder
                .RegisterType<EfRepositoryFactory>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<EfUnitOfWorkFactory>()
                .AsImplementedInterfaces()
                .WithParameter(new TypedParameter(typeof(string), _connectionstring))
                .SingleInstance();


            builder
                .RegisterAssemblyTypes(typeof(DepositService).Assembly)
                .InNamespaceOf<DepositService>()
                .AsImplementedInterfaces()
                .SingleInstance();


        }
    }
}
