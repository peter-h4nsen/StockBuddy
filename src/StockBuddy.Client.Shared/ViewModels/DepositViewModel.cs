using System;
using System.Collections.ObjectModel;
using StockBuddy.Domain.Entities;
using System.Collections.Generic;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class DepositViewModel : ViewModelBase
    {
        public DepositViewModel()
        {
            Trades = new ObservableCollection<TradeViewModel>();
            Dividends = new ObservableCollection<DividendViewModel>();
            StockPositions = new ObservableCollection<StockPositionViewModel>();
            SellableStockIds = new int[0];
        }

        public int Id { get; set; }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { Set(ref _description, value, true, () => ValidateText(Description, "Beskrivelse", 100)); }
        }

        private string _identityNumber;
        public string IdentityNumber
        {
            get { return _identityNumber; }
            set { Set(ref _identityNumber, value, true, () => ValidateText(IdentityNumber, "Depotnummer", 30)); }
        }

        private DepositTypes _depositType;
        public DepositTypes DepositType
        {
            get { return _depositType; }
            set { Set(ref _depositType, value, true); }
        }

        public ObservableCollection<TradeViewModel> Trades { get; }
        public ObservableCollection<DividendViewModel> Dividends { get; }
        public ObservableCollection<StockPositionViewModel> StockPositions { get; }
        
        public IEnumerable<int> SellableStockIds { get; set; }

        // OBS. Lavet for at kunne lave en instans med default værdier sat.
        // Properties kan ikke sættes i ctor, da de i nogle tilfælde bliver sat igen
        // efter den er kaldt, hvorved vm'en bliver dirty da både den oprindelige
        // default værdi og den nye findes i changetracker. (Sker hvis model mappes til vm f.eks.).
        // Nyt: Skal måske laves anderledes ? :D
        public static DepositViewModel CreateDefault()
        {
            return new DepositViewModel
            {
                Description = string.Empty,
                IdentityNumber = string.Empty,
                DepositType = DepositTypes.ÅbentDepot
            };
        }
    }
}
