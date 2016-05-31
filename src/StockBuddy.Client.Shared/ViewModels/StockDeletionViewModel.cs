using System;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.Messaging;
using StockBuddy.Client.Shared.Messaging.Messages;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class StockDeletionViewModel : ViewModelBase
    {
        private readonly IStockGateway _stockGateway;
        private readonly ISharedDataProvider _sharedDataProvider;
        private readonly IViewService _viewService;
        private readonly IMessagebus _messagebus;
        private Action onEditSucceeded;

        public StockDeletionViewModel(IStockGateway stockGateway, ISharedDataProvider sharedDataProvider, 
            IViewService viewService, IMessagebus messagebus)
        {
            Guard.AgainstNull(() => stockGateway, () => sharedDataProvider, () => viewService, () => messagebus);

            _stockGateway = stockGateway;
            _sharedDataProvider = sharedDataProvider;
            _viewService = viewService;
            _messagebus = messagebus;

            DeleteCommand = new RelayCommand(Delete, CanDelete);
            PageTitle = "Sletning af papir";
            IsBackNavigationEnabled = true;
        }

        protected override void NavigatedTo(object parameter)
        {
            var tuple = parameter as Tuple<StockViewModel, Action>;
            var stockToDelete = tuple?.Item1;
            onEditSucceeded = tuple?.Item2;

            if (stockToDelete == null)
                throw new ArgumentException("Must supply stock to delete");

            Stock = stockToDelete;
            IsStockReferenced = _stockGateway.IsStockReferenced(Stock.Id);
        }

        public RelayCommand DeleteCommand { get; }

        private StockViewModel _stock;
        public StockViewModel Stock
        {
            get { return _stock; }
            set { Set(ref _stock, value); }
        }

        private bool _isStockReferenced;
        public bool IsStockReferenced
        {
            get { return _isStockReferenced; }
            set { Set(ref _isStockReferenced, value); }
        }

        private void Delete()
        {
            if (!CanDelete())
                return;

            if (IsStockReferenced)
            {
                Stock.IsActive = false;
                _stockGateway.Update(Stock);
                onEditSucceeded();
            }
            else
            {
                _sharedDataProvider.DeleteStock(Stock);
            }

            _viewService.NavigateTo(typeof(StockManagementViewModel));
            CleanUp();
        }

        private bool CanDelete() => Stock != null;

        protected override void NavigatedBack()
        {
            CleanUp();
        }

        private void CleanUp()
        {
            Stock = null;
        }
    }
}
