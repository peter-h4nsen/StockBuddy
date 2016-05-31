using System;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class DepositOverviewViewModel : ViewModelBase
    {
        private readonly IViewService _viewService;

        public DepositOverviewViewModel(IViewService viewService)
        {
            Guard.AgainstNull(() => viewService);

            _viewService = viewService;

            CreateTradeCommand = new RelayCommand(() => NavigateToTradeEditor(Deposit, null, null));
            CreateBuyTradeCommand = new ParameterizedRelayCommand<TradeViewModel>(t => NavigateToTradeEditor(Deposit, true, t.Stock.Id));
            CreateSellTradeCommand = new ParameterizedRelayCommand<TradeViewModel>(t => NavigateToTradeEditor(Deposit, false, t.Stock.Id));
            NavigateToDividendManagementCommand = new RelayCommand(() => _viewService.NavigateTo(typeof(DividendManagementViewModel), Deposit));
            NavigateToTaxInfoCommand = new RelayCommand(() => _viewService.NavigateTo(typeof(YearlyReportViewModel), Deposit));

            PageTitle = "DEPOTOVERSIGT";
            IsBackNavigationEnabled = true;
        }
        
        protected override void NavigatedTo(object parameter)
        {
            var depositViewModel = parameter as DepositViewModel;

            if (depositViewModel == null)
                throw new ArgumentException($"Must pass {nameof(DepositViewModel)} when navigating to {nameof(DepositOverviewViewModel)}");

            Deposit = depositViewModel;
        }

        public RelayCommand CreateTradeCommand { get; }
        public ParameterizedRelayCommand<TradeViewModel> CreateBuyTradeCommand { get; }
        public ParameterizedRelayCommand<TradeViewModel> CreateSellTradeCommand { get; }
        public RelayCommand NavigateToDividendManagementCommand { get; }
        public RelayCommand NavigateToTaxInfoCommand { get; }

        private DepositViewModel _deposit;
        public DepositViewModel Deposit
        {
            get { return _deposit; }
            set { Set(ref _deposit, value); }
        }

        private void NavigateToTradeEditor(DepositViewModel deposit, bool? selectBuy, int? stockId)
        { 
            _viewService.NavigateTo(
                typeof(TradeEditorViewModel), 
                new TradeEditorViewModel.NavigationArgs(deposit, selectBuy, stockId));
        }
    }
}
