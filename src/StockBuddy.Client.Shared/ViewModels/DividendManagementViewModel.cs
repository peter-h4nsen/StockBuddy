using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;
using System.Collections.ObjectModel;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class DividendManagementViewModel : ViewModelBase
    {
        private readonly IDepositGateway _depositGateway;

        public DividendManagementViewModel(IDepositGateway depositGateway, ISharedDataProvider sharedDataProvider)
        {
            Guard.AgainstNull(() => depositGateway, () => sharedDataProvider);

            _depositGateway = depositGateway;
            SharedDataProvider = sharedDataProvider;

            CalculateDividendsCommand = new RelayCommand(CalculateDividends);
            AddDividendCommand = new ParameterizedRelayCommand<DividendViewModel>(AddDividend);
            DeleteDividendCommand = new ParameterizedRelayCommand<DividendViewModel>(DeleteDividend);
            CalculatedDividends = new ObservableCollection<DividendViewModel>();

            PageTitle = "Udbyttebetalinger";
            IsBackNavigationEnabled = true;
        }

        protected override void NavigatedTo(object parameter)
        {
            var deposit = parameter as DepositViewModel;

            if (deposit == null)
                throw new ArgumentException($"Must pass args when navigating to {nameof(DividendManagementViewModel)}");

            Deposit = deposit;
            DividendsCollectionView = new CollectionViewSource { Source = deposit.Dividends }.View;
            DividendsCollectionView.Filter = DividendsFilter;

            CalculatedDividends.Clear();
        }

        public RelayCommand CalculateDividendsCommand { get; }
        public ParameterizedRelayCommand<DividendViewModel> AddDividendCommand { get; }
        public ParameterizedRelayCommand<DividendViewModel> DeleteDividendCommand { get; }

        public DepositViewModel Deposit { get; private set; }
        public ISharedDataProvider SharedDataProvider { get; }
        public ObservableCollection<DividendViewModel> CalculatedDividends { get; }
        
        private ICollectionView _dividendsCollectionView;
        public ICollectionView DividendsCollectionView
        {
            get { return _dividendsCollectionView; }
            private set { Set(ref _dividendsCollectionView, value); }
        }

        private int? _selectedYear;
        public int? SelectedYear
        {
            get { return _selectedYear; }
            set { Set(ref _selectedYear, value); }
        }

        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (Set(ref _filterText, value))
                    DividendsCollectionView.Refresh();
            }
        }

        private bool DividendsFilter(object item)
        {
            var dividend = (DividendViewModel)item;

            var filterPassed =
                (string.IsNullOrWhiteSpace(FilterText) || dividend.FilterValue.Contains(FilterText.ToLower()));

            return filterPassed;
        }

        private void CalculateDividends()
        {
            if (SelectedYear == null)
                return;

            var dividends = 
                _depositGateway.CalculateDividends(SelectedYear.Value, Deposit, SharedDataProvider.GeneralMeetings);

            CalculatedDividends.Clear();

            foreach (var dividend in dividends)
            {
                dividend.ShowCreatedInfo = true;
                CalculatedDividends.Add(dividend);
            }
        }

        private void AddDividend(DividendViewModel dividendVm)
        {
            _depositGateway.CreateDividend(dividendVm);
            dividendVm.ShowCreatedInfo = false;
            Deposit.Dividends.Add(dividendVm);
            CalculatedDividends.Remove(dividendVm);
        }

        private void DeleteDividend(DividendViewModel dividendVm)
        {
            _depositGateway.DeleteDividend(dividendVm.Id);
            Deposit.Dividends.Remove(dividendVm);

            // If the deleted dividend is shown in the calculated view, make it possible to add it again.
            var calculated = CalculatedDividends.SingleOrDefault(p => p.GeneralMeeting.Id == dividendVm.GeneralMeeting.Id);

            if (calculated != null)
            {
                calculated.IsDifferent = false;
                calculated.IsCreated = false;
            }
        }
    }
}
