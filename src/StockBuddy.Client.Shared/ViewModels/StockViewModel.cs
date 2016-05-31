using System;
using System.Collections.ObjectModel;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class StockViewModel : ViewModelBase
    {
        public StockViewModel()
        {
            Trades = new ObservableCollection<TradeViewModel>();
            StockSplits = new ObservableCollection<StockSplitViewModel>();
        }

        public int Id { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value, true, () => ValidateText(Name, "Navn", 50)); }
        }

        private string _symbol;
        public string Symbol
        {
            get { return _symbol; }
            set { Set(ref _symbol, value, true, () => ValidateText(Symbol, "Symbol", 30)); }
        }

        private string _isin;
        public string Isin
        {
            get { return _isin; }
            set { Set(ref _isin, value, true, () => ValidateText(Isin, "Isin", 12, true)); }
        }

        private StockTypes _stockType;
        public StockTypes StockType
        {
            get { return _stockType; }
            set { Set(ref _stockType, value, true); }
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set { Set(ref _isActive, value); }
        }

        public ObservableCollection<TradeViewModel> Trades { get; }
        public ObservableCollection<StockSplitViewModel> StockSplits { get; }

        
        public static StockViewModel CreateDefault()
        {
            return new StockViewModel
            {
                Name = string.Empty,
                Symbol = string.Empty,
                Isin = string.Empty,
                StockType = StockTypes.Aktie,
                IsActive = true
            };
        }
    }
}
