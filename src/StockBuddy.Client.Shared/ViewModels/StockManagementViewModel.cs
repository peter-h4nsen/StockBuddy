using System;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.Generic;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class StockManagementViewModel : ViewModelBase
    {
        private readonly IViewService _viewService;
        
        public StockManagementViewModel(IViewService viewService, ISharedDataProvider sharedDataProvider)
        {
            Guard.AgainstNull(() => viewService, () => sharedDataProvider);

            _viewService = viewService;
            
            EditCurrentStockCommand = new RelayCommand(EditCurrentStock, IsCurrentStockSelected);
            DeleteCurrentStockCommand = new RelayCommand(DeleteCurrentStock, IsCurrentStockSelected);
            
            StocksCollectionView = new CollectionViewSource { Source = sharedDataProvider.Stocks }.View;
            StocksCollectionView.Filter = StocksFilter;

            PageTitle = "Opsætning af papirer";
            IsBackNavigationEnabled = true;
        }

        public RelayCommand EditCurrentStockCommand { get; }
        public RelayCommand DeleteCurrentStockCommand { get; }

        public ICollectionView StocksCollectionView { get; }
        public IDictionary<string, Enum> StockStatusFilterChoices { get; } = typeof(StockStatusFilters).GetDescriptions();
        public IDictionary<string, Enum> StockTypeFilterChoices { get; } = typeof(StockTypeFilters).GetDescriptions();


        private StockViewModel _currentStock;
        public StockViewModel CurrentStock
        {
            get { return _currentStock; }
            set { Set(ref _currentStock, value, new[] { EditCurrentStockCommand, DeleteCurrentStockCommand }); }
        }

        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set { SetAndRefreshFilter(ref _filterText, value); }
        }

        private StockStatusFilters _selectedStockStatusFilter;
        public StockStatusFilters SelectedStockStatusFilter
        {
            get { return _selectedStockStatusFilter; }
            set { SetAndRefreshFilter(ref _selectedStockStatusFilter, value); }
        }

        private StockTypeFilters _selectedStockTypeFilter;
        public StockTypeFilters SelectedStockTypeFilter
        {
            get { return _selectedStockTypeFilter; }
            set { SetAndRefreshFilter(ref _selectedStockTypeFilter, value); }
        }

        private void SetAndRefreshFilter<T>(ref T field, T value)
        {
            if (Set(ref field, value))
            {
                StocksCollectionView.Refresh();
            }
        }

        private bool StocksFilter(object item)
        {
            var stock = (StockViewModel)item;

            var filterPassed = !(
                (SelectedStockStatusFilter == StockStatusFilters.ShowOnlyActive && !stock.IsActive) ||
                (SelectedStockStatusFilter == StockStatusFilters.ShowOnlyInactive && stock.IsActive) ||
                (SelectedStockTypeFilter == StockTypeFilters.ShowOnlyAktie && stock.StockType != StockTypes.Aktie) ||
                (SelectedStockTypeFilter == StockTypeFilters.ShowOnlyTegningsretAktie && stock.StockType != StockTypes.TegningsretAktie) ||
                (SelectedStockTypeFilter == StockTypeFilters.ShowOnlyUdenlandskAktie && stock.StockType != StockTypes.UdenlandskAktie) ||
                (SelectedStockTypeFilter == StockTypeFilters.ShowOnlyInvForening && stock.StockType != StockTypes.Investeringsbevis) ||
                (!string.IsNullOrWhiteSpace(FilterText) && !stock.Name.ToLower().Contains(FilterText.ToLower()))
            );

            return filterPassed;
        }

        private bool IsCurrentStockSelected() => CurrentStock != null;

        private void EditCurrentStock() => NavigateTo(typeof(StockEditorViewModel));
        private void DeleteCurrentStock() => NavigateTo(typeof(StockDeletionViewModel));
        
        private void NavigateTo(Type vmType)
        {
            if (!IsCurrentStockSelected())
                return;

            _viewService.NavigateTo(vmType, Tuple.Create<StockViewModel, Action>(CurrentStock, StocksCollectionView.Refresh));
        }
            
        
        public enum StockStatusFilters : byte
        {
            [Description("Vis alle")]
            ShowAll,

            [Description("Vis kun aktive")]
            ShowOnlyActive,

            [Description("Vis kun inaktive")]
            ShowOnlyInactive
        }

        public enum StockTypeFilters : byte
        {
            [Description("Vis alle")]
            ShowAll,

            [Description("Vis kun aktier")]
            ShowOnlyAktie = StockTypes.Aktie,

            [Description("Vis kun tegningsretter")]
            ShowOnlyTegningsretAktie = StockTypes.TegningsretAktie,

            [Description("Vis kun udenlandske aktier")]
            ShowOnlyUdenlandskAktie = StockTypes.UdenlandskAktie,

            [Description("Vis kun investeringsbeviser")]
            ShowOnlyInvForening = StockTypes.Investeringsbevis
        }
    }
}
