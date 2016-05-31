using System;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.Messaging;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;
using System.Collections.Generic;
using StockBuddy.Domain.Entities;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class StockEditorViewModel : ViewModelBase
    {
        private readonly IStockGateway _stockGateway;
        private readonly ISharedDataProvider _sharedDataProvider;
        private readonly IViewService _viewService;
        private readonly IMessagebus _messagebus;
        private bool _isEdittingStock;
        private Action onEditSucceeded;

        public StockEditorViewModel(IStockGateway stockGateway, ISharedDataProvider sharedDataProvider, IViewService viewService, IMessagebus messagebus)
        {
            Guard.AgainstNull(() => stockGateway, () => sharedDataProvider, () => viewService, () => messagebus);

            _stockGateway = stockGateway;
            _sharedDataProvider = sharedDataProvider;
            _viewService = viewService;
            _messagebus = messagebus;

            SaveStockCommand = new RelayCommand(SaveStock, CanSaveStock);
            IsBackNavigationEnabled = true;
        }

        protected override void NavigatedTo(object parameter)
        {
            var tuple = parameter as Tuple<StockViewModel, Action>;
            var stockToEdit = tuple?.Item1;
            onEditSucceeded = tuple?.Item2;

            Stock = stockToEdit ?? StockViewModel.CreateDefault();
            Stock.ReevaluateOnDirtyStateAndValidationChanges(SaveStockCommand);
            _isEdittingStock = stockToEdit != null;
            PageTitle = _isEdittingStock ? "Ret eksisterende papir" : "Opret nyt papir";
        }

        public RelayCommand SaveStockCommand { get; }
        public IDictionary<string, Enum> StockTypeChoices { get; } = typeof(StockTypes).GetDescriptions();

        private StockViewModel _stock;
        public StockViewModel Stock
        {
            get { return _stock; }
            set { Set(ref _stock, value, SaveStockCommand.Yield()); }
        }

        private bool CanSaveStock()
        {
            return Stock != null && Stock.CanSave;
        }

        private void SaveStock()
        {
            if (!CanSaveStock())
                return;

            var success = Validate(_viewService, () =>
            {
                if (_isEdittingStock)
                    _stockGateway.Update(Stock);
                else
                    _sharedDataProvider.AddStock(Stock);
            });

            if (!success)
                return;

            Stock.CommitChanges();

            if (_isEdittingStock)
                onEditSucceeded();

            Stock = null;
            _viewService.NavigateTo(typeof(StockManagementViewModel));
        }

        protected override void NavigatedBack()
        {
            if (Stock != null)
            {
                Stock.UndoChanges();
                Stock = null;
            }
        }
    }
}
