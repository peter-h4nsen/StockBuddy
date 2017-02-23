using System;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using System.Threading.Tasks;

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
            UpdateHistoricalStockInfo().Forget();
        }

        private async Task UpdateHistoricalStockInfo()
        {
            bool result = await _stockGateway.UpdateHistoricalStockInfo();

            if (!result)
            {
                ViewService.DisplayValidationErrors("Error while updating stock prices.");
            }
        }
    }
}
