using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class GeneralMeetingViewModel : ViewModelBase
    {
        public int Id { get; set; }

        private DateTime? _meetingDate;
        public DateTime? MeetingDate
        {
            get { return _meetingDate; }
            set { Set(ref _meetingDate, value, true, () => ValidateDate(MeetingDate, "Dato")); }
        }

        private StockViewModel _stock = new StockViewModel();
        public StockViewModel Stock
        {
            get { return _stock; }
            set { Set(ref _stock, value, true, () => ValidateNotNull(Stock, "Der skal vælges et papir")); }
        }

        private decimal _dividendRate;
        public decimal DividendRate
        {
            get { return _dividendRate; }
            set { Set(ref _dividendRate, value, true, () => ValidateNumber(DividendRate, "Udbytte", 0)); }
        }

        public string FilterValue
        {
            get { return $"{Stock.Name} {MeetingDate.Value.ToString("d")} {DividendRate.ToString()}".ToLower(); }
        }

        public static GeneralMeetingViewModel CreateDefault()
        {
            return new GeneralMeetingViewModel
            {
                MeetingDate = DateTime.Today,
                Stock = null,
                DividendRate = 0
            };
        }
    }
}