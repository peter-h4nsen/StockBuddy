using System;
using Autofac;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.Messaging;
using StockBuddy.Client.Shared.Services;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;
using StockBuddy.Shared.Utilities.AppSettings;
using StockBuddy.Domain.Settings;
using StockBuddy.DataAccess.Webservices.YahooFinance;
using StockBuddy.Domain.Repositories;
using StockBuddy.Client.Shared.ViewModels;

namespace StockBuddy.Client.Shared.Bootstrapping
{
    public sealed class AppBootstrapper
    {
        private readonly IViewService _viewService;
        private readonly string _connectionstring;
        private readonly string _providerName;

        public AppBootstrapper(IViewService viewService, string connectionstring, string providerName)
        {
            Guard.AgainstNull(() => viewService, () => connectionstring, () => providerName);

            _viewService = viewService;
            _connectionstring = connectionstring;
            _providerName = providerName;
        }

        public HostViewModel Run()
        {
            var container = CreateContainer();

            _viewService.Init(container.Resolve);

            ViewModelLocator.SetViewModelFactory(container.Resolve);
            GlobalCommands.SetViewService(_viewService);

            return container.Resolve<HostViewModel>();
        }

        private IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new ViewModelModule());
            builder.RegisterModule(new GatewayModule(_connectionstring, _providerName));

            builder.RegisterInstance(_viewService);

            builder.RegisterType<Messagebus>().As<IMessagebus>().SingleInstance();
            builder.RegisterType<SharedDataProvider>().As<ISharedDataProvider>().SingleInstance();
            builder.RegisterType<YahooFinanceStockInfoRepository>().As<IStockInfoRetrieverRepository>().SingleInstance();

            builder.RegisterInstance(CreateSettingsProvider()).AsImplementedInterfaces().SingleInstance();

            return builder.Build();
        }

        private IAppSettingsProvider<GlobalSettings> CreateSettingsProvider()
        {
            return 
                new AppSettingsProvider<GlobalSettings>(
                    new SqlServerSettingsStore(_connectionstring));
        }
    }
}
