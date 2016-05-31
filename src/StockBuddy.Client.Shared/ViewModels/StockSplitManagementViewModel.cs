using System;
using System.Collections.ObjectModel;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.Misc;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class StockSplitManagementViewModel : ViewModelBase
    {
        private readonly IStockGateway _stockGateway;
        private readonly IViewService _viewService;

        public StockSplitManagementViewModel(IStockGateway stockGateway, IViewService viewService, ISharedDataProvider sharedDataProvider)
        {
            Guard.AgainstNull(() => stockGateway, () => viewService, () => sharedDataProvider);

            _stockGateway = stockGateway;
            _viewService = viewService;
            SharedDataProvider = sharedDataProvider;

            SaveCommand = new RelayCommand(Save, IsSaveEnabled);
            ResetCommand = new RelayCommand(Reset);
            BeginEditCommand = new ParameterizedRelayCommand<StockSplitViewModel>(BeginEdit);
            BeginDeleteCommand = new ParameterizedRelayCommand<StockSplitViewModel>(BeginDelete);
            StockSplits = new ObservableCollection<StockSplitViewModel>(_stockGateway.GetAllStockSplits());

            Reset();
            PageTitle = "Aktiesplits";
            IsBackNavigationEnabled = true;
        }

        public ISharedDataProvider SharedDataProvider { get; }
        public ObservableCollection<StockSplitViewModel> StockSplits { get; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand ResetCommand { get; }
        public ParameterizedRelayCommand<StockSplitViewModel> BeginEditCommand { get; }
        public ParameterizedRelayCommand<StockSplitViewModel> BeginDeleteCommand { get; }
        
        private StockSplitViewModel _selectedStockSplit;
        public StockSplitViewModel SelectedStockSplit
        {
            get { return _selectedStockSplit; }
            set
            {
                if (_selectedStockSplit != null)
                    _selectedStockSplit.ChangeNotificationEnabled = true;

                if (value != null)
                    value.ChangeNotificationEnabled = false;

                Set(ref _selectedStockSplit, value, SaveCommand.Yield());
            }
        }

        private ModificationStates _modificationState;
        public ModificationStates ModificationState
        {
            get { return _modificationState; }
            set
            {
                if (Set(ref _modificationState, value))
                    RaisePropertyChanged(nameof(CanResetModification), nameof(IsDeleting));
            }
        }

        private string _oldStockFilterText;
        public string OldStockFilterText
        {
            get { return _oldStockFilterText; }
            set { Set(ref _oldStockFilterText, value); }
        }

        private string _newStockFilterText;
        public string NewStockFilterText
        {
            get { return _newStockFilterText; }
            set { Set(ref _newStockFilterText, value); }
        }

        public bool CanResetModification => ModificationState != ModificationStates.Add;
        public bool IsDeleting => ModificationState == ModificationStates.Delete;

        private void BeginEdit(StockSplitViewModel stockSplit) => SetModificationState(stockSplit, ModificationStates.Edit);
        private void BeginDelete(StockSplitViewModel stockSplit) => SetModificationState(stockSplit, ModificationStates.Delete);

        private void Reset()
        {
            SetModificationState(StockSplitViewModel.CreateDefault(), ModificationStates.Add);
            OldStockFilterText = NewStockFilterText = string.Empty;
        }

        private void SetModificationState(StockSplitViewModel vm, ModificationStates state)
        {
            if (SelectedStockSplit != null && ModificationState == ModificationStates.Edit)
                SelectedStockSplit.UndoChanges();

            ModificationState = state;
            SelectedStockSplit = vm;
            SelectedStockSplit.ReevaluateOnDirtyStateAndValidationChanges(SaveCommand);
        }

        private bool IsSaveEnabled() => 
            (SelectedStockSplit != null) && 
            (ModificationState == ModificationStates.Delete || SelectedStockSplit.CanSave);

        private void Save()
        {
            var stockSplit = SelectedStockSplit;

            if (ModificationState == ModificationStates.Delete)
            {
                _stockGateway.DeleteStockSplit(stockSplit.Id);
                StockSplits.Remove(stockSplit);
            }
            else
            {
                var success = Validate(_viewService, () =>
                {
                    if (ModificationState == ModificationStates.Add)
                    {
                        _stockGateway.CreateStockSplit(stockSplit);
                        StockSplits.Add(stockSplit);
                    }
                    else if (ModificationState == ModificationStates.Edit)
                    {
                        _stockGateway.UpdateStockSplit(stockSplit);
                    }
                });

                if (!success)
                    return;

                stockSplit.CommitChanges();
            }

            Reset();
        }

        protected override void NavigatedBack()
        {
            Reset();
        }
    }
}
