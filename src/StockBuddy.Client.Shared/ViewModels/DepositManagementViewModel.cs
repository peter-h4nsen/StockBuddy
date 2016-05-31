using System;
using System.Collections.ObjectModel;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.Messaging;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class DepositManagementViewModel : ViewModelBase
    {
        private readonly IDepositGateway _depositGateway;
        private readonly IViewService _viewService;
        private readonly IMessagebus _messagebus;

        public DepositManagementViewModel(IDepositGateway depositGateway, IViewService viewService, 
            IMessagebus messagebus, ISharedDataProvider sharedDataProvider)
        {
            Guard.AgainstNull(() => depositGateway, () => viewService, () => messagebus, () => sharedDataProvider);

            _viewService = viewService;
            _depositGateway = depositGateway;
            _messagebus = messagebus;

            EditSelectedDepositCommand = new RelayCommand(EditSelectedDeposit, IsDepositSelected);
            DeleteSelectedDepositCommand = new RelayCommand(DeleteSelectedDeposit, IsDepositSelected);
            ShowDepositOverviewCommand = new RelayCommand(ShowDepositOverview, IsDepositSelected);
            Deposits = sharedDataProvider.Deposits;

            PageTitle = "Depotadministration";
        }

        public RelayCommand EditSelectedDepositCommand { get; }
        public RelayCommand DeleteSelectedDepositCommand { get; }
        public RelayCommand ShowDepositOverviewCommand { get; }
        public ObservableCollection<DepositViewModel> Deposits { get; }

        private DepositViewModel _selectedDeposit;
        public DepositViewModel SelectedDeposit
        {
            get { return _selectedDeposit; }
            set
            {
                Set(ref _selectedDeposit, value, 
                    new[] { EditSelectedDepositCommand, DeleteSelectedDepositCommand, ShowDepositOverviewCommand });
            }
        }

        private void EditSelectedDeposit() => NavigateTo(typeof(DepositEditorViewModel));
        private void DeleteSelectedDeposit() => NavigateTo(typeof(DepositDeletionViewModel));
        private void ShowDepositOverview() => NavigateTo(typeof(DepositOverviewViewModel));

        private bool IsDepositSelected() => SelectedDeposit != null;

        private void NavigateTo(Type vmType)
        {
            if (!IsDepositSelected())
                return;

            _viewService.NavigateTo(vmType, SelectedDeposit);
        }
    }
}
