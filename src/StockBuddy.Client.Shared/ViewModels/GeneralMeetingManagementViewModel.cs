using System;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Shared.Utilities;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.Misc;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class GeneralMeetingManagementViewModel : ViewModelBase
    {
        private readonly IStockGateway _stockGateway;
        private readonly IViewService _viewService;
        
        public GeneralMeetingManagementViewModel(IStockGateway stockGateway, IViewService viewService, ISharedDataProvider sharedDataProvider)
        {
            Guard.AgainstNull(() => stockGateway, () => viewService, () => sharedDataProvider);

            _stockGateway = stockGateway;
            _viewService = viewService;
            SharedDataProvider = sharedDataProvider;

            SaveCommand = new RelayCommand(Save, IsSaveEnabled);
            ResetCommand = new RelayCommand(Reset);
            BeginEditCommand = new ParameterizedRelayCommand<GeneralMeetingViewModel>(BeginEdit);
            BeginDeleteCommand = new ParameterizedRelayCommand<GeneralMeetingViewModel>(BeginDelete);

            GeneralMeetingsCollectionView = new CollectionViewSource { Source = sharedDataProvider.GeneralMeetings }.View;
            GeneralMeetingsCollectionView.Filter = GeneralMeetingsFilter;

            Reset();
            PageTitle = "Generalforsamlinger";
            IsBackNavigationEnabled = true;
        }

        public ISharedDataProvider SharedDataProvider { get; }
        public ICollectionView GeneralMeetingsCollectionView { get; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand ResetCommand { get; }
        public ParameterizedRelayCommand<GeneralMeetingViewModel> BeginEditCommand { get; }
        public ParameterizedRelayCommand<GeneralMeetingViewModel> BeginDeleteCommand { get; }

        private GeneralMeetingViewModel _selectedGeneralMeeting;
        public GeneralMeetingViewModel SelectedGeneralMeeting
        {
            get { return _selectedGeneralMeeting; }
            set
            {
                if (_selectedGeneralMeeting != null)
                    _selectedGeneralMeeting.ChangeNotificationEnabled = true;

                if (value != null)
                    value.ChangeNotificationEnabled = false;

                Set(ref _selectedGeneralMeeting, value, SaveCommand.Yield());
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

        private string _stockFilterText;
        public string StockFilterText
        {
            get { return _stockFilterText; }
            set { Set(ref _stockFilterText, value); }
        }

        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (Set(ref _filterText, value))
                {
                    GeneralMeetingsCollectionView.Refresh();
                }
            }
        }

        private bool GeneralMeetingsFilter(object item)
        {
            var generalMeeting = (GeneralMeetingViewModel)item;

            var filterPassed =
                (string.IsNullOrWhiteSpace(FilterText) || generalMeeting.FilterValue.Contains(FilterText.ToLower()));
            
            return filterPassed;
        }

        public bool CanResetModification => ModificationState != ModificationStates.Add;
        public bool IsDeleting => ModificationState == ModificationStates.Delete;

        private void BeginEdit(GeneralMeetingViewModel vm) => SetModificationState(vm, ModificationStates.Edit);
        private void BeginDelete(GeneralMeetingViewModel vm) => SetModificationState(vm, ModificationStates.Delete);
        
        private void Reset()
        {
            SetModificationState(GeneralMeetingViewModel.CreateDefault(), ModificationStates.Add);
            StockFilterText = string.Empty;
        }

        private void SetModificationState(GeneralMeetingViewModel vm, ModificationStates state)
        {
            if (SelectedGeneralMeeting != null && ModificationState == ModificationStates.Edit)
                SelectedGeneralMeeting.UndoChanges();

            ModificationState = state;
            SelectedGeneralMeeting = vm;
            SelectedGeneralMeeting.ReevaluateOnDirtyStateAndValidationChanges(SaveCommand);
        }

        private bool IsSaveEnabled() =>
            (SelectedGeneralMeeting != null) &&
            (ModificationState == ModificationStates.Delete || SelectedGeneralMeeting.CanSave);
        
        private void Save()
        {
            var generalMeeting = SelectedGeneralMeeting;

            if (ModificationState == ModificationStates.Delete)
            {
                SharedDataProvider.DeleteGeneralMeeting(generalMeeting);
            }
            else
            {
                var success = Validate(_viewService, () =>
                {
                    if (ModificationState == ModificationStates.Add)
                        SharedDataProvider.AddGeneralMeeting(generalMeeting);
                    else if (ModificationState == ModificationStates.Edit)
                        SharedDataProvider.UpdateGeneralMeeting(generalMeeting);
                });

                if (!success)
                    return;

                generalMeeting.CommitChanges();
            }

            Reset();
        }

        protected override void NavigatedBack()
        {
            Reset();
        }
    }
}
