using System;
using System.Collections.Generic;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Domain.Entities;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class DepositEditorViewModel : ViewModelBase
    {
        private readonly IDepositGateway _depositGateway;
        private readonly IViewService _viewService;
        private readonly ISharedDataProvider _sharedDataProvider;
        private bool _isEdittingDeposit;

        public DepositEditorViewModel(IDepositGateway depositGateway, IViewService viewService, ISharedDataProvider sharedDataProvider)
        {
            Guard.AgainstNull(() => depositGateway, () => viewService, () => sharedDataProvider);

            _depositGateway = depositGateway;
            _viewService = viewService;
            _sharedDataProvider = sharedDataProvider;

            SaveDepositCommand = new RelayCommand(SaveDeposit, CanSaveDeposit);
            IsBackNavigationEnabled = true;
        }

        protected override void NavigatedTo(object parameter)
        {
            var depositToEdit = parameter as DepositViewModel;
            Deposit = depositToEdit ?? DepositViewModel.CreateDefault();
            Deposit.ReevaluateOnDirtyStateAndValidationChanges(SaveDepositCommand);
            _isEdittingDeposit = depositToEdit != null;
            PageTitle = _isEdittingDeposit ? "Ret eksisterende depot" : "Opret nyt depot";
        }

        public RelayCommand SaveDepositCommand { get; }
        public IDictionary<string, Enum> DepositTypeChoices { get; } = typeof(DepositTypes).GetDescriptions();

        private DepositViewModel _deposit;
        public DepositViewModel Deposit
        {
            get { return _deposit; }
            set { Set(ref _deposit, value, SaveDepositCommand.Yield()); }
        }

        private bool CanSaveDeposit()
        {
            return Deposit != null && Deposit.CanSave;
        }

        private void SaveDeposit()
        {
            if (!CanSaveDeposit())
                return;

            var success = Validate(_viewService, () =>
            {
                if (_isEdittingDeposit)
                    _depositGateway.Update(Deposit);
                else
                    _sharedDataProvider.AddDeposit(Deposit);
            });

            if (!success)
                return;

            Deposit.CommitChanges();
            Deposit = null;
            _viewService.NavigateTo(typeof(DepositManagementViewModel));
        }

        protected override void NavigatedBack()
        {
            Deposit.UndoChanges();
            Deposit = null;
        }
    }
}
