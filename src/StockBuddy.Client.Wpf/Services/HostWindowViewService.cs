using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Client.Shared.ViewModels;
using StockBuddy.Shared.Utilities;
using StockBuddy.Client.Wpf.Views;
using System.Windows.Input;

namespace StockBuddy.Client.Wpf.Services
{
    public sealed class HostWindowViewService : DependencyObject, IViewService
    {
        private const string ViewModelSuffix = "Model";

        private readonly Window _rootWindow;
        private readonly IDictionary<Type, AdornerDecorator> _viewCache;
        private readonly IDictionary<Type, Tuple<Type, object>> _parameterCache;
        private readonly Stack<Type> _navigationStack;
        private readonly Type[] _viewTypes;

        private Func<Type, object> _viewModelFactory;
        private ViewModelBase _currentViewModel;
        private ViewModelBase _nextViewModel;

        public ObservableCollection<GlobalButtonViewModel> GlobalButtons { get; private set; }
        public RelayCommand NavigateBackCommand { get; private set; }

        public HostWindowViewService(Window rootWindow)
        {
            Guard.AgainstNull(() => rootWindow);

            _rootWindow = rootWindow;
            _viewCache = new Dictionary<Type, AdornerDecorator>();
            _parameterCache = new Dictionary<Type, Tuple<Type, object>>();
            _navigationStack = new Stack<Type>();

            _viewTypes = (
                from type in typeof(HostWindowViewService).Assembly.GetTypes()
                where type.Name.EndsWith("View")
                select type).ToArray();

            GlobalButtons = new ObservableCollection<GlobalButtonViewModel>();
            NavigateBackCommand = new RelayCommand(NavigateBack, () => CanCurrentViewNavigateBack);
        }

        public void Init(Func<Type, object> viewModelFactory)
        {
            Guard.AgainstNull(() => viewModelFactory);
            _viewModelFactory = viewModelFactory;
        }

        public void SetGlobalButtons(IEnumerable<GlobalButtonViewModel> globalButtons)
        {
            GlobalButtons.Clear();

            foreach (var globalButton in globalButtons)
                GlobalButtons.Add(globalButton);
        }

        public void DisplayValidationErrors(string message)
        {
            var messageDialog = new MessageDialog(message);
            messageDialog.Owner = _rootWindow;
            messageDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            messageDialog.ShowDialog();
        }

        /// <summary>
        /// Kaldes for at skifte til et andet view. Parameter er typen på den viewmodel man vil have vist.
        /// Det passende view findes ved at generere det matchende navn ud fra viewmodelnavnet, og 
        /// lave en instans af typen med det genererede navn, hvorefter viewet sættes som content i root vinduet.
        /// Desuden laves en instans af viewmodel'en, og denne sættes som datacontext på viewet.
        /// Navn genereres f.eks. som: CustomerListViewModel -> CustomerListView.
        /// </summary>
        public void NavigateTo(Type viewModelType, object parameter = null)
        {
            Guard.AgainstNull(() => viewModelType);

            if (_viewModelFactory == null)
                throw new InvalidOperationException("ViewModelFactory not defined. Must call 'Init' before use.");

            if (_currentViewModel != null && _currentViewModel.GetType() == viewModelType)
                return;

            AdornerDecorator view;

            if (!_viewCache.TryGetValue(viewModelType, out view))
            {
                var viewModelName = viewModelType.Name;

                if (!viewModelName.EndsWith(ViewModelSuffix))
                {
                    throw new InvalidOperationException(
                        string.Format("Can't navigate to view. Invalid viewmodel name: '{0}'", viewModelName));
                }

                var viewName = viewModelName.Substring(0, viewModelName.Length - ViewModelSuffix.Length);
                var viewType = _viewTypes.SingleOrDefault(p => p.Name == viewName);

                if (viewType == null)
                {
                    throw new InvalidOperationException(
                        string.Format("Can't navigate to view. No view found with name: '{0}'", viewName));
                }

                var userControl = Activator.CreateInstance(viewType) as UserControl;

                if (userControl == null)
                {
                    throw new InvalidOperationException(
                        string.Format("Can't navigate to view. Found view is not a usercontrol: '{0}'", viewName));
                }

                // Tilføjer selve viewet (usercontrollen) til en AdornerDecorator, som så bruges som content.
                // I nogle tilfælde virker ting der benytter adorner layer ikke korrekt ellers. (F.eks. validationerrortemplates).
                view = new AdornerDecorator();
                view.Child = userControl;
                view.DataContext = _viewModelFactory(viewModelType);
                _viewCache[viewModelType] = view;
            }

            var viewModel = view.DataContext as ViewModelBase;

            if (viewModel == null)
                throw new InvalidOperationException(
                    string.Format("The views datacontext is not a viewmodel: '{0}'", viewModelType.Name));

            _nextViewModel = viewModel;

            parameter = FindAndUpdateCachedParameter(parameter, viewModelType);
            viewModel.WasNavigatedTo(parameter);
            UpdateNavigationStack(viewModel);
            UpdateSelectedGlobalButton(viewModelType);
            CurrentPageTitle = (viewModel.PageTitle ?? "MISSING PAGE TITLE!!").ToUpper();
            CurrentPageSubTitle = viewModel.PageSubTitle;
            CanCurrentViewNavigateBack = viewModel.IsBackNavigationEnabled;
            CurrentView = view;

            _currentViewModel = viewModel;
            _nextViewModel = null;
        }

        public void NavigateBack()
        {
            if (!_navigationStack.Any())
                return;

            _currentViewModel.WasNavigatedBack();

            var viewModelType = _navigationStack.Peek();
            NavigateTo(viewModelType);
        }

        private void UpdateNavigationStack(ViewModelBase newViewModel)
        {
            if (newViewModel.IsBackNavigationEnabled)
            {
                var newViewModelType = newViewModel.GetType();

                // Hvis den vm der hoppes hen på findes i stacken i forvejen, går man tilbage.
                // Ellers går man frem på en ny vm, som skal tilføjes til stacken.
                if (_navigationStack.Contains(newViewModelType))
                {
                    // Fjern fra stacken indtil den der hoppes tilbage til er fjernet.
                    // Hvis man kun går et step tilbage fjernes kun et element, men man kan også navigere flere steps tilbage, 
                    // hvorved de views der springes over også skal fjernes fra stacken.
                    Type poppedViewModelType = null;

                    while (poppedViewModelType != newViewModelType)
                        poppedViewModelType = _navigationStack.Pop();
                }
                else
                {
                    if (_currentViewModel != null)
                        _navigationStack.Push(_currentViewModel.GetType());
                }
            }
            else
            {
                _navigationStack.Clear();
            }
        }

        private object FindAndUpdateCachedParameter(object parameter, Type viewModelType)
        {
            // Gemmer den seneste parameter for viewmodel typen, og benytter denne hvis den nuværende parameter er null.
            // Dette tillader at man kan gå fra et view der modtager en parameter til et "globalt" view 
            // og tilbage igen uden at den oprindelige parameter går tabt.
            if (parameter != null)
            {
                _parameterCache[viewModelType] = Tuple.Create(_currentViewModel.GetType(), parameter);
            }
            else
            {
                Tuple<Type, object> paramInfo;

                if (_parameterCache.TryGetValue(viewModelType, out paramInfo))
                {
                    // En gemt parameter er fundet. Bruger den dog kun hvis det ikke er viewet med den oprindelige parameter vi kommer fra igen.
                    // Man kan f.eks. komme fra et management-view til et editor-view med og uden param (ret/opret ny), 
                    // og i dette tilfælde skal den gemte parameter ikke bruges igen.
                    var previousViewModelType = paramInfo.Item1;
                    var cachedParameter = paramInfo.Item2;

                    if (previousViewModelType != _currentViewModel.GetType())
                        parameter = cachedParameter;
                    else
                        _parameterCache.Remove(viewModelType);
                }
            }

            return parameter;
        }

        private void UpdateSelectedGlobalButton(Type viewModelType)
        {
            var isNextViewModelGlobal = IsViewModelGlobal(viewModelType);

            if (!isNextViewModelGlobal)
            {
                // Nulstil global knap hvis der navigeres væk fra et globalt view og hent på et ikke-globalt.
                if (_currentViewModel != null && IsViewModelGlobal(_currentViewModel.GetType()))
                    SelectedGlobalButton = null;
            }
            else
            {
                // Der navigeres hen på et globalt view via et direkte kald til NavigateTo metoden (altså uden at klikke på en global knap).
                // I dette tilfælde skal knappen for det globale view sættes manuelt, så den bliver selected i GUI'en.
                if (SelectedGlobalButton == null || !SelectedGlobalButton.HasViewModelAttached(viewModelType))
                    SelectedGlobalButton = GlobalButtons.Single(p => p.HasViewModelAttached(viewModelType));
            }
        }

        private bool IsViewModelGlobal(Type viewModelType)
        {
            return GlobalButtons.Any(p => p.HasViewModelAttached(viewModelType));
        }

        #region "CurrentView" Dependency property

        public object CurrentView
        {
            get { return (object)GetValue(CurrentViewProperty); }
            set { SetValue(CurrentViewProperty, value); }
        }

        public static readonly DependencyProperty CurrentViewProperty =
            DependencyProperty.Register("CurrentView", typeof(object), typeof(HostWindowViewService), new PropertyMetadata(null));

        #endregion

        #region "CanCurrentViewNavigateBack" Dependency property

        public bool CanCurrentViewNavigateBack
        {
            get { return (bool)GetValue(CanCurrentViewNavigateBackProperty); }
            set { SetValue(CanCurrentViewNavigateBackProperty, value); }
        }

        public static readonly DependencyProperty CanCurrentViewNavigateBackProperty =
            DependencyProperty.Register("CanCurrentViewNavigateBack", typeof(bool), typeof(HostWindowViewService),
            new PropertyMetadata(false, CanCurrentViewNavigateBackChanged));

        private static void CanCurrentViewNavigateBackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewService = (HostWindowViewService)d;
            viewService.NavigateBackCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region "CurrentPageTitle" Dependency property

        public string CurrentPageTitle
        {
            get { return (string)GetValue(CurrentPageTitleProperty); }
            set { SetValue(CurrentPageTitleProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageTitleProperty =
            DependencyProperty.Register("CurrentPageTitle", typeof(string), typeof(HostWindowViewService), new PropertyMetadata(null));

        #endregion

        #region "CurrentPageSubTitle" Dependency property

        public string CurrentPageSubTitle
        {
            get { return (string)GetValue(CurrentPageSubTitleProperty); }
            set { SetValue(CurrentPageSubTitleProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageSubTitleProperty =
            DependencyProperty.Register("CurrentPageSubTitle", typeof(string), typeof(HostWindowViewService), new PropertyMetadata(null));

        #endregion

        #region "SelectedGlobalButton" Dependency property

        public GlobalButtonViewModel SelectedGlobalButton
        {
            get { return (GlobalButtonViewModel)GetValue(SelectedGlobalButtonProperty); }
            set { SetValue(SelectedGlobalButtonProperty, value); }
        }

        public static readonly DependencyProperty SelectedGlobalButtonProperty =
            DependencyProperty.Register("SelectedGlobalButton", typeof(GlobalButtonViewModel), typeof(HostWindowViewService),
            new PropertyMetadata(null, SelectedGlobalButtonPropertyChanged));

        private static void SelectedGlobalButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var globalButton = (GlobalButtonViewModel)e.NewValue;

            if (globalButton == null)
                return;

            var viewService = (HostWindowViewService)d;

            // Den valgte globale knap navigerer til samme view som det view der er igang med at blive vist, så der skal ikke gøres noget.
            // Sker hvis der navigeres til et globalt view via kode (uden at trykke på en knap), og SelectedGlobalButton så sættes 
            // for at den rigtige knap bliver valgt i GUI'en.
            if (viewService._nextViewModel != null && globalButton.HasViewModelAttached(viewService._nextViewModel.GetType()))
                return;

            viewService.NavigateTo(globalButton.NavigatedViewModel);
        }

        #endregion

    }
}
