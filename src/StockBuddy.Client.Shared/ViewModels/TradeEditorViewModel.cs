using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Domain.DTO;
using StockBuddy.Shared.Utilities;
using System.Windows.Data;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class TradeEditorViewModel : ViewModelBase
    {
        private readonly ITradeGateway _tradeGateway;
        private readonly IViewService _viewService;
        private readonly ISharedDataProvider _sharedDataProvider;

        public TradeEditorViewModel(ITradeGateway tradeGateway, IViewService viewService, ISharedDataProvider sharedDataProvider)
        {
            Guard.AgainstNull(() => tradeGateway, () => viewService, () => sharedDataProvider);
            _tradeGateway = tradeGateway;
            _viewService = viewService;
            _sharedDataProvider = sharedDataProvider;

            SaveTradeCommand = new RelayCommand(SaveTrade, CanSaveTrade);
            LatestTrades = new ObservableCollection<TradeViewModel>();

            Stocks = sharedDataProvider.Stocks;
            StocksCollectionView = new CollectionViewSource { Source = Stocks }.View;
            StocksCollectionView.Filter = StocksFilter;

            PageTitle = "OPRET NY HANDEL";
            IsBackNavigationEnabled = true;
        }

        protected override void NavigatedTo(object parameter)
        {
            var args = parameter as NavigationArgs;

            if (args == null)
                throw new ArgumentException($"Must pass args when navigating to {nameof(TradeEditorViewModel)}");

            Deposit = args.DepositViewModel;

            Trade = TradeViewModel.CreateDefault();
            Trade.Deposit = Deposit;
            Trade.ReevaluateOnDirtyStateAndValidationChanges(SaveTradeCommand);
            Trade.PropertyChanged += Trade_PropertyChanged;

            if (args.SelectBuy != null)
            {
                if (args.SelectBuy == true)
                    Trade.IsBuy = true;
                else
                    Trade.IsSale = true;
            }

            if (args.StockId != null)
            {
                SelectStock(args.StockId.Value);
            }

            RaisePropertyChanged(nameof(InfoMessage));
        }

        public RelayCommand SaveTradeCommand { get; }
        public ObservableCollection<StockViewModel> Stocks { get; }
        public ObservableCollection<TradeViewModel> LatestTrades { get; }
        public ICollectionView StocksCollectionView { get; }
        public DepositViewModel Deposit { get; private set; }

        private TradeViewModel _trade;
        public TradeViewModel Trade
        {
            get { return _trade; }
            set { Set(ref _trade, value, SaveTradeCommand.Yield()); }
        }

        private TradeInfoDto _tradeInfo;
        public TradeInfoDto TradeInfo
        {
            get { return _tradeInfo; }
            set { Set(ref _tradeInfo, value); }
        }

        private bool _isShowingAllDepositsLatestTrades = true;
        public bool IsShowingAllDepositsLatestTrades
        {
            get { return _isShowingAllDepositsLatestTrades; }
            set
            {
                if (Set(ref _isShowingAllDepositsLatestTrades, value))
                    UpdateLatestTrades();
            }
        }

        private string _stockFilterText = string.Empty;
        public string StockFilterText
        {
            get { return _stockFilterText; }
            set { Set(ref _stockFilterText, value); }
        }

        public string InfoMessage
        {
            get
            {
                if (Stocks != null && !Stocks.Any())
                {
                    return "Der er ingen papirer oprettet i systemet";
                }
                else if (Trade != null)
                {
                    if (!Trade.IsBuyOrSale)
                    {
                        return "Vælg enten 'KØB' eller 'SALG' for at oprette en handel";
                    }
                    else if (Trade.IsSale == true && !Deposit.SellableStockIds.Any())
                    {
                        return "Der er ingen åbne positioner";
                    }
                }

                return null;
            }
        }

        void Trade_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var propName = e.PropertyName;

            if (propName == nameof(Trade.IsBuy) || propName == nameof(Trade.IsSale))
            {
                // Hvis køb bliver true sættes salg automatisk til false (og omvendt),
                // hvilket resulterer i propertychanged events for både køb og salg.
                // Ved første event har begge properties dog samme værdi, så fortsætter kun
                // hvis de har forskellige værdier (ved andet event).
                if (Trade.IsBuy != Trade.IsSale)
                {
                    RaisePropertyChanged(nameof(InfoMessage));
                    UpdateStocks();
                    UpdateTradeInfo();
                }
            }

            if (propName == nameof(Trade.Stock) || propName == nameof(Trade.Price) || propName == nameof(Trade.Quantity))
            {
                UpdateTradeInfo();
            }

            if (propName == nameof(Trade.Stock))
            {
                UpdateLatestTrades();

                if (Trade.IsStockDefined)
                    Trade.MaximumSaleQuantity = TradeInfo.CurrentStockQuantity;
            }
        }

        private void SelectStock(int stockId)
        {
            var stockToSelect = Stocks.Where(p => p.Id == stockId).FirstOrDefault();

            if (stockToSelect != null)
            {
                if (StocksFilter(stockToSelect))
                {
                    StockFilterText = stockToSelect.Name;
                    Trade.Stock = stockToSelect;
                }
            }
        }

        private void UpdateStocks()
        {
            StocksCollectionView.Refresh();

            if (Trade.IsStockDefined)
            {
                // Det valgte papir bliver ikke nulstillet automatisk når filteret ændres og
                // papiret ikke længere kan vælges, så nulstiller det manuelt.
                if (!StocksFilter(Trade.Stock))
                {
                    StockFilterText = string.Empty;
                    Trade.Stock = null;
                }
            }
        }

        private void UpdateTradeInfo()
        {
            if (Trade.IsStockDefined)
            {
                TradeInfo = _tradeGateway.GetTradeInfo(Trade, Deposit);
            }
        }

        private void UpdateLatestTrades()
        {
            if (Trade.IsStockDefined)
            {
                var trades = Trade.Stock.Trades.AsEnumerable();

                if (!IsShowingAllDepositsLatestTrades)
                    trades = trades.Where(p => p.Deposit.Id == Deposit.Id);

                trades = trades.OrderByDescending(p => p.TradeDate).Take(5);

                LatestTrades.Clear();

                foreach (var trade in trades)
                    LatestTrades.Add(trade);
            }
        }

        private bool StocksFilter(object item)
        {
            var stock = (StockViewModel)item;

            if (!stock.IsActive)
                return false;

            if (Trade != null && Trade.IsSale == true)
            {
                return Deposit.SellableStockIds.Contains(stock.Id);
            }

            return true;
        }

        private bool CanSaveTrade()
        {
            return Trade != null && Trade.CanSave;
        }

        private void SaveTrade()
        {
            if (!CanSaveTrade())
                return;

            var success = Validate(_viewService, () =>
            {
                _tradeGateway.Create(Trade);
            });

            if (!success)
                return;

            Trade.CommitChanges();

            _sharedDataProvider.RefreshDepositTradeAdded(Deposit, Trade);

            _viewService.NavigateTo(typeof(DepositOverviewViewModel));
            CleanUp(false);
        }

        protected override void NavigatedBack()
        {
            CleanUp(true);
        }

        private void CleanUp(bool undo)
        {
            if (Trade != null)
            {
                if (undo)
                    Trade.UndoChanges();

                Trade = null;
            }

            StockFilterText = string.Empty;
        }

        public sealed class NavigationArgs
        {
            public NavigationArgs(DepositViewModel depositViewModel, bool? selectBuy, int? stockId)
            {
                DepositViewModel = depositViewModel;
                SelectBuy = selectBuy;
                StockId = stockId;
            }

            public DepositViewModel DepositViewModel { get; }
            public bool? SelectBuy { get; }
            public int? StockId { get; }
        }
    }
}
