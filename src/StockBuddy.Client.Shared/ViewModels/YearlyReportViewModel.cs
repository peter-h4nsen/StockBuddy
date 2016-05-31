using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Shared.Utilities;
using StockBuddy.Domain.DTO.YearlyReport;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class YearlyReportViewModel : ViewModelBase
    {
        private readonly IDepositGateway _depositGateway;

        private readonly ObservableCollection<YearlyReportStockGroupViewModel> _stockGroups;
        private readonly ObservableCollection<YearlyReportDividendDTO> _dividends;

        public YearlyReportViewModel(IDepositGateway depositGateway)
        {
            Guard.AgainstNull(() => depositGateway);

            _depositGateway = depositGateway;


            CalculateCommand = new RelayCommand(Calculate);
            ExpandAllCommand = new RelayCommand(() => ToggleExpanded(true));
            CollapseAllCommand = new RelayCommand(() => ToggleExpanded(false));

            _stockGroups = new ObservableCollection<YearlyReportStockGroupViewModel>();
            _dividends = new ObservableCollection<YearlyReportDividendDTO>();

            StockGroupsCollectionView = new CollectionViewSource { Source = _stockGroups }.View;
            StockGroupsCollectionView.Filter = (item) => Filter(((YearlyReportStockGroupViewModel)item).FilterValue, StockGroupFilterText);

            DividendsCollectionView = new CollectionViewSource { Source = _dividends }.View;
            DividendsCollectionView.Filter = (item) => Filter(((YearlyReportDividendDTO)item).FilterValue, DividendFilterText);

            //TODO: Fix
            TaxYears = new ObservableCollection<KeyValuePair<string, int>>();
            TaxYears.Add(new KeyValuePair<string, int>("2016", 2016));
            TaxYears.Add(new KeyValuePair<string, int>("2015", 2015));
            TaxYears.Add(new KeyValuePair<string, int>("2014", 2014));

            PageTitle = "Skatteoplysninger";
            IsBackNavigationEnabled = true;
        }

        protected override void NavigatedTo(object parameter)
        {
            var deposit = parameter as DepositViewModel;

            if (deposit == null)
                throw new ArgumentException($"Must pass args when navigating to {nameof(YearlyReportViewModel)}");

            Deposit = deposit;

            // TODO: Clear noget?
        }

        public RelayCommand CalculateCommand { get; }
        public RelayCommand ExpandAllCommand { get; }
        public RelayCommand CollapseAllCommand { get; }

        public DepositViewModel Deposit { get; private set; }

        public ICollectionView StockGroupsCollectionView { get; }
        public ICollectionView DividendsCollectionView { get; }

        public ObservableCollection<KeyValuePair<string, int>> TaxYears { get; }

        private YearlyReportDTO _yearlyReport;
        public YearlyReportDTO YearlyReport
        {
            get { return _yearlyReport; }
            set { Set(ref _yearlyReport, value); }
        }

        private int? _selectedYear;
        public int? SelectedYear
        {
            get { return _selectedYear; }
            set { Set(ref _selectedYear, value); }
        }

        private bool _isMarried;
        public bool IsMarried
        {
            get { return _isMarried; }
            set { Set(ref _isMarried, value); }
        }

        private string _stockGroupFilterText;
        public string StockGroupFilterText
        {
            get { return _stockGroupFilterText; }
            set
            {
                if (Set(ref _stockGroupFilterText, value))
                    StockGroupsCollectionView.Refresh();
            }
        }

        private string _dividendFilterText;
        public string DividendFilterText
        {
            get { return _dividendFilterText; }
            set
            {
                if (Set(ref _dividendFilterText, value))
                    DividendsCollectionView.Refresh();
            }
        }

        private bool Filter(string filterValue, string filterText)
        {
            return
                string.IsNullOrWhiteSpace(filterText) ||
                filterValue.Contains(filterText.ToLower());
        }

        private void ToggleExpanded(bool isExpanded)
        {
            foreach (var parent in _stockGroups)
            {
                parent.IsExpanded = isExpanded;
            }
        }

        private void Calculate()
        {
            if (SelectedYear == null)
                return;

            var yearlyReport = _depositGateway.GetYearlyReport(SelectedYear.Value, IsMarried, Deposit);
            
            _stockGroups.Refill(yearlyReport.StockGroups.Select(p => new YearlyReportStockGroupViewModel(p)));
            _dividends.Refill(yearlyReport.Dividends);
            YearlyReport = yearlyReport;
        }
    }
}
