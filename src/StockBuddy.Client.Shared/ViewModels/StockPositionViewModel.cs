using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class StockPositionViewModel : ViewModelBase
    {

        private StockViewModel _stock;
        public StockViewModel Stock
        {
            get { return _stock; }
            set { Set(ref _stock, value); }
        }

        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set { Set(ref _quantity, value); }
        }
    }
}
