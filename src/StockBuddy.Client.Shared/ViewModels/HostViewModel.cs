using System;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class HostViewModel : ViewModelBase
    {
        public HostViewModel(IViewService viewService)
        {
            Guard.AgainstNull(() => viewService);

            ViewService = viewService;
        }

        public IViewService ViewService { get; }
    }
}
