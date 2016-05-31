using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class DividendViewModel : ViewModelBase
    {
        public int Id { get; set; }

        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set { Set(ref _quantity, value, true); }
        }

        private GeneralMeetingViewModel _generalMeeting;
        public GeneralMeetingViewModel GeneralMeeting
        {
            get { return _generalMeeting; }
            set { Set(ref _generalMeeting, value, true); }
        }

        private DepositViewModel _deposit;
        public DepositViewModel Deposit
        {
            get { return _deposit; }
            set { Set(ref _deposit, value, true); }
        }

        private bool _isCreated;
        public bool IsCreated
        {
            get { return _isCreated; }
            set { Set(ref _isCreated, value); }
        }

        public bool _isDifferent;
        public bool IsDifferent
        {
            get { return _isDifferent; }
            set { Set(ref _isDifferent, value); }
        }

        public decimal DividendPayment { get; set; }
        public bool ShowCreatedInfo { get; set; }

        public string FilterValue
        {
            get
            {
                return $@"{
                    GeneralMeeting.Stock.Name} {
                    GeneralMeeting.MeetingDate.Value.ToString("d")} {
                    GeneralMeeting.DividendRate} {
                    Quantity} {
                    DividendPayment}".ToLower();
            }
        }
    }
}
