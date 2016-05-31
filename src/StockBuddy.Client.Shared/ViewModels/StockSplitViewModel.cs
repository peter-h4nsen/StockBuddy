using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class StockSplitViewModel : ViewModelBase
    {
        public int Id { get; set; }

        private DateTime? _date;
        public DateTime? Date
        {
            get { return _date; }
            set { Set(ref _date, value, true, () => ValidateDate(Date, "Dato")); }
        }

        private StockViewModel _oldStock = new StockViewModel();
        public StockViewModel OldStock
        {
            get { return _oldStock; }
            set { Set(ref _oldStock, value, true, () => ValidateNotNull(OldStock, "Der skal vælges et oprindeligt papir")); }
        }

        private StockViewModel _newStock = new StockViewModel();
        public StockViewModel NewStock
        {
            get { return _newStock; }
            set { Set(ref _newStock, value, true, () => ValidateNotNull(NewStock, "Der skal vælges et nyt papir")); }
        }

        private int _ratioFrom;
        public int RatioFrom
        {
            get { return _ratioFrom; }
            set { Set(ref _ratioFrom, value, true, () => ValidateNumber(RatioFrom, "Forhold (fra)", 1, 10)); }
        }

        private int _ratioTo;
        public int RatioTo
        {
            get { return _ratioTo; }
            set { Set(ref _ratioTo, value, true, () => ValidateNumber(RatioTo, "Forhold (til)", 1, 10)); } 
        }

        public static StockSplitViewModel CreateDefault()
        {
            return new StockSplitViewModel
            {
                Date = DateTime.Today,
                OldStock = null,
                NewStock = null,
                RatioFrom = 1,
                RatioTo = 1
            };
        }
    }
}
