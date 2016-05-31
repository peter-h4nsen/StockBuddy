using System;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.Messaging;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Client.Shared.Messaging.Messages;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class DepositDeletionViewModel : ViewModelBase
    {
        private readonly IDepositGateway _depositGateway;
        private readonly IViewService _viewService;
        private readonly ISharedDataProvider _sharedDataProvider;

        public DepositDeletionViewModel(IDepositGateway depositGateway, IViewService viewService, ISharedDataProvider sharedDataProvider)
        {
            Guard.AgainstNull(() => depositGateway, () => viewService, () => sharedDataProvider);

            _depositGateway = depositGateway;
            _viewService = viewService;
            _sharedDataProvider = sharedDataProvider;

            DeleteCommand = new RelayCommand(Delete, CanDelete);
            PageTitle = "Sletning af depot";
            IsBackNavigationEnabled = true;
        }

        protected override void NavigatedTo(object parameter)
        {
            var depositToDelete = parameter as DepositViewModel;

            if (depositToDelete == null)
                throw new ArgumentException("Must supply a deposit to delete");

            Deposit = depositToDelete;
        }

        public RelayCommand DeleteCommand { get; }

        private DepositViewModel _deposit;
        public DepositViewModel Deposit
        {
            get { return _deposit; }
            set { Set(ref _deposit, value); }
        }

        private string _confirmationText;
        public string ConfirmationText
        {
            get { return _confirmationText; }
            set { Set(ref _confirmationText, value, DeleteCommand.Yield()); }
        }

        private bool CanDelete()
        {
            return Deposit != null && ConfirmationText == "SLET";
        }

        private void Delete()
        {
            if (!CanDelete())
                return;

            _sharedDataProvider.DeleteDeposit(Deposit);
            _viewService.NavigateTo(typeof(DepositManagementViewModel));
            CleanUp();
        }

        protected override void NavigatedBack()
        {
            CleanUp();
        }

        private void CleanUp()
        {
            Deposit = null;
            ConfirmationText = null;
        }
    }
}
