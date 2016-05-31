using System;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class TradeViewModel : ViewModelBase
    {
        public int Id { get; set; }
        public DepositViewModel Deposit { get; set; }

        private bool? _isBuy;
        public bool? IsBuy
        {
            get { return _isBuy; }
            set
            {
                if (Set(ref _isBuy, value))
                {
                    ValidateBuySale();
                    RaisePropertyChanged(nameof(IsBuyOrSale));
                    RaisePropertyChanged(nameof(MaximumTradeQuantity));
                }
            }
        }

        private bool? _isSale;
        public bool? IsSale
        {
            get { return _isSale; }
            set
            {
                if (Set(ref _isSale, value))
                {
                    ValidateBuySale();
                    RaisePropertyChanged(nameof(IsBuyOrSale));
                    RaisePropertyChanged(nameof(MaximumTradeQuantity));
                    AdjustQuantity();
                }
            }
        }

        private int? _quantity;
        public int? Quantity
        {
            get { return _quantity; }
            set { Set(ref _quantity, value, true, () => ValidateNumber(Quantity, "Antal", 1, MaximumTradeQuantity)); }
        }

        private decimal? _price;
        public decimal? Price
        {
            get { return _price; }
            set { Set(ref _price, value, true, () => ValidateNumber(Price, "Pris", 0)); }
        }

        private decimal? _commision;
        public decimal? Commission
        {
            get { return _commision; }
            set { Set(ref _commision, value, true, () => ValidateNumber(Commission, "Omkostninger", 0)); }
        }

        private DateTime? _tradeDate;
        public DateTime? TradeDate
        {
            get { return _tradeDate; }
            set { Set(ref _tradeDate, value, true, () => ValidateDate(TradeDate, "Handelsdato")); }
        }

        private StockViewModel _stock = new StockViewModel();
        public StockViewModel Stock
        {
            get { return _stock; }
            set
            {
                if (Set(ref _stock, value, true, () => ValidateNotNull(Stock, "Der skal vælges et papir")))
                {
                    RaisePropertyChanged(nameof(IsStockDefined));
                }
            }
        }

        private int _maximumSaleQuantity;
        public int MaximumSaleQuantity
        {
            get { return _maximumSaleQuantity; }
            set
            {
                if (Set(ref _maximumSaleQuantity, value))
                {
                    AdjustQuantity();
                    RaisePropertyChanged(nameof(MaximumTradeQuantity));
                }
            }
        }

        private bool IsMaximumSaleQuantityRelevant
        {
            get { return IsSale == true && MaximumSaleQuantity > 0; }
        }

        public int MaximumTradeQuantity
        {
            get { return IsMaximumSaleQuantityRelevant ? MaximumSaleQuantity : 10000000; }
        }

        public bool IsBuyOrSale
        {
            get { return IsBuy == true || IsSale == true; }
        }

        public bool IsStockDefined
        {
            get { return Stock != null; }
        }

        public string InfoText
        {
            get
            {
                if (IsBuyOrSale && TradeDate != null && Quantity != null && Price != null)
                {
                    return string.Format("{0} - {1} {2:N0} stk. til kurs {3:N2}",
                        TradeDate.Value.ToShortDateString(),
                        IsBuy.Value ? "Købt" : "Solgt",
                        Quantity.Value,
                        Price.Value);
                }

                return string.Empty;
            }
        }

        private void AdjustQuantity()
        {
            if (IsMaximumSaleQuantityRelevant)
            {
                if (Quantity > MaximumSaleQuantity)
                    Quantity = MaximumSaleQuantity;
            }
        }

        private void ValidateBuySale()
        {
            string brokenRule = null;

            if (!IsBuyOrSale)
                brokenRule = "Der skal vælges enten Køb eller Salg";

            SetBrokenValidationRule(nameof(IsSale), brokenRule);
            SetBrokenValidationRule(nameof(IsBuy), brokenRule);
        }

        public static TradeViewModel CreateDefault()
        {
            return new TradeViewModel
            {
                IsBuy = false,
                IsSale = false,
                Stock = null,
                Quantity = 1,
                Price = 0,
                Commission = 0,
                TradeDate = DateTime.Today
            };
        }
    }
}
