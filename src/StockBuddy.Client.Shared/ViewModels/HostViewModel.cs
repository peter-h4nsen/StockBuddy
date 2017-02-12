using System;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;
using StockBuddy.Client.Shared.DomainGateways.Contracts;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class HostViewModel : ViewModelBase
    {
        private readonly IStockGateway _stockGateway;

        public HostViewModel(IViewService viewService, IStockGateway stockGateway)
        {
            Guard.AgainstNull(() => viewService, () => stockGateway);

            ViewService = viewService;
            _stockGateway = stockGateway;
        }

        public IViewService ViewService { get; }

        public void OnAppStarted()
        {
            _stockGateway.Test();
        }
    }
}
