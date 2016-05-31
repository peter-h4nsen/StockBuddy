using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.Services.Contracts;
using StockBuddy.Domain.Validation;
using StockBuddy.Client.Shared.Misc;

namespace StockBuddy.Client.Shared.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {

        #region Navigation

        public string PageTitle { get; protected set; }
        public string PageSubTitle { get; protected set; }
        public bool IsBackNavigationEnabled { get; protected set; }

        public void WasNavigatedTo(object parameter)
        {
            NavigatedTo();
            NavigatedTo(parameter);
            RaisePropertyChanged(nameof(IsNavigatedTo));
        }

        public void WasNavigatedBack()
        {
            NavigatedBack();
        }

        // Subclasses can override these to run something when navigated to/back.
        protected virtual void NavigatedTo() { }
        protected virtual void NavigatedTo(object parameter) { }
        protected virtual void NavigatedBack() { }

        // Views can bind to this to get notified when navigated to.
        public bool IsNavigatedTo => true;

        #endregion


        #region Property-setters

        public event PropertyChangedEventHandler PropertyChanged;
        public bool ChangeNotificationEnabled { get; set; } = true;

        protected bool Set<T>(
            //Expression<Func<T>> propertyExpression,
            ref T field, T newValue,
            [CallerMemberName]string propertyName = "")
        {
            return Set(ref field, newValue, false, null, null, null, propertyName);
        }

        protected bool Set<T>(
            //Expression<Func<T>> propertyExpression,
            ref T field, T newValue,
            IEnumerable<Command> reevaluatedCommands,
            [CallerMemberName]string propertyName = "")
        {
            return Set(ref field, newValue, false, null, null, reevaluatedCommands, propertyName);
        }

        protected bool Set<T>(
            //Expression<Func<T>> propertyExpression,
            ref T field, T newValue,
            bool updateDirtyState,
            [CallerMemberName]string propertyName = "")
        {
            return Set(ref field, newValue, updateDirtyState, null, null, null, propertyName);
        }

        protected bool Set<T>(
            //Expression<Func<T>> propertyExpression,
            ref T field, T newValue,
            Action beforeValueAppliedAction,
            [CallerMemberName]string propertyName = "")
        {
            return Set(ref field, newValue, false, null, beforeValueAppliedAction, null, propertyName);
        }

        protected bool Set<T>(
            //Expression<Func<T>> propertyExpression,
            ref T field, T newValue,
            bool updateDirtyState,
            Func<IEnumerable<string>> brokenRulesFunc,
            [CallerMemberName]string propertyName = "")
        {
            return Set(ref field, newValue, updateDirtyState, brokenRulesFunc, null, null, propertyName);
        }

        protected bool Set<T>(
            //Expression<Func<T>> propertyExpression,
            ref T field, T newValue,
            bool updateDirtyState,
            Func<IEnumerable<string>> brokenRulesFunc,
            Action beforeValueAppliedAction,
            IEnumerable<Command> reevaluatedCommands,
            [CallerMemberName]string propertyName = "")
        {
            var isNewValue = !EqualityComparer<T>.Default.Equals(field, newValue);

            if (isNewValue)
            {     
                if (beforeValueAppliedAction != null)
                    beforeValueAppliedAction();

                field = newValue;

                if (ChangeNotificationEnabled)
                    RaisePropertyChanged(propertyName);

                if (updateDirtyState)
                    UpdateDirtyState(newValue, propertyName);

                if (brokenRulesFunc != null)
                {
                    var brokenRules = brokenRulesFunc();
                    SetBrokenValidationRules(propertyName, brokenRules);
                }
            }

            ReevaluateCommands(reevaluatedCommands);

            return isNewValue;
        }

        //protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        //{
        //    var propertyName = GetPropertyName(propertyExpression);
        //    RaisePropertyChanged(propertyName);
        //}

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //protected string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        //{
        //    if (propertyExpression == null)
        //        throw new ArgumentNullException("propertyExpression");

        //    var body = propertyExpression.Body as MemberExpression;

        //    if (body == null)
        //        throw new ArgumentException("Argument is not a member", "propertyExpression");

        //    var property = body.Member as PropertyInfo;

        //    if (property == null)
        //        throw new ArgumentException("Argument is not a property", "propertyExpression");

        //    return property.Name;
        //}

        #endregion


        #region Validation

        private readonly IDictionary<string, List<string>> _brokenValidationRules = new Dictionary<string, List<string>>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged = delegate { };

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            List<string> validationErrors = null;
            _brokenValidationRules.TryGetValue(propertyName, out validationErrors);
            return validationErrors;
        }

        public bool HasErrors
        {
            get { return _brokenValidationRules.Count > 0; }
        }

        protected void SetBrokenValidationRule(string propertyName, string brokenRule)
        {
            var brokenRules = new List<string>();

            if (brokenRule != null)
                brokenRules.Add(brokenRule);

            SetBrokenValidationRules(propertyName, brokenRules);
        }

        private void SetBrokenValidationRules(string propertyName, IEnumerable<string> brokenRules)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return;

            var brokenRulesLocal = (brokenRules ?? Enumerable.Empty<string>()).ToList();
            var hasNewBrokenRules = brokenRulesLocal.Any();
            var hasOldBrokenRules = _brokenValidationRules.ContainsKey(propertyName);

            if (hasNewBrokenRules || hasOldBrokenRules)
            {
                if (hasNewBrokenRules)
                    _brokenValidationRules[propertyName] = brokenRulesLocal;
                else
                    _brokenValidationRules.Remove(propertyName);

                RaiseErrorsChanged(propertyName);
            }
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected bool Validate(IViewService viewService, Action action)
        {
            if (action == null)
                return true;

            try
            {
                action();
                return true;
            }
            catch (EntityValidationException ex)
            {
                string validationMessage = null;

                if (ex.BrokenValidationRules != null)
                {
                    foreach (var brokenRule in ex.BrokenValidationRules)
                    {
                        if (!string.IsNullOrWhiteSpace(brokenRule))
                            validationMessage += brokenRule + Environment.NewLine + Environment.NewLine;
                    }
                }

                viewService.DisplayValidationErrors(
                    validationMessage ?? "Unknown validation error occured.");

                return false;
            }
        }

        protected IEnumerable<string> ValidateText(string text, string fieldName, int length, bool isExactLength = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                yield return string.Format("{0} skal være udfyldt", fieldName);
                yield break;
            }

            if (isExactLength)
            {
                if (text.Length != length)
                    yield return string.Format("{0} skal være på nøjagtigt {1} karakterer", fieldName, length);
            }
            else
            {
                if (text.Length > length)
                    yield return string.Format("{0} må maks være på {1} karakterer", fieldName, length);
            }
        }

        protected IEnumerable<string> ValidateNumber(decimal? number, string fieldName, decimal minValue, decimal? maxValue = null)
        {
            if (number == null)
            {
                yield return string.Format("{0} skal have en værdi", fieldName);
                yield break;
            }

            if (number.Value < minValue)
            {
                yield return string.Format("{0} må ikke være lavere end {1}", fieldName, minValue);
            }
            else if (maxValue != null && number.Value > maxValue)
            {
                yield return string.Format("{0} må ikke være højere end {1}", fieldName, maxValue.Value);
            }
        }

        protected IEnumerable<string> ValidateDate(DateTime? date, string fieldName)
        {
            if (date == null)
            {
                yield return string.Format("{0} skal have en værdi", fieldName);
                yield break;
            }
        }

        protected IEnumerable<string> ValidateNotNull(object obj, string message)
        {
            if (obj == null)
            {
                yield return message;
                yield break;
            }
        }

        #endregion


        #region Dirty-flag tracking

        /* Tracks if properties on the viewmodel have changed.
           The dictionary holds a propertyname and the state for that property.
           If the original value and the current value are the same, the property is not dirty.
           If at least one property is dirty, the whole viewmodel is considered dirty. */

        private readonly IDictionary<string, DirtyState> _dirtyStates = new Dictionary<string, DirtyState>();
        public event Action DirtyStateChanged = delegate { };

        private void UpdateDirtyState(object value, string propertyName)
        {
            var wasDirty = IsDirty;

            if (_dirtyStates.ContainsKey(propertyName))
                _dirtyStates[propertyName].CurrentValue = value;
            else
                _dirtyStates[propertyName] = new DirtyState(value, value);

            // Only raise event if the update caused the state to change.
            if (wasDirty != IsDirty)
                RaiseDirtyStateChanged();
        }

        public bool IsDirty
        {
            get { return _dirtyStates.Any(p => p.Value.IsDirty); }
        }

        public void CommitChanges()
        {
            ChangeNotificationEnabled = true;

            foreach (var dirtyState in _dirtyStates)
            {
                if (dirtyState.Value.IsDirty)
                {
                    dirtyState.Value.CommitChange();
                    RaisePropertyChanged(dirtyState.Key);
                }
            }
        }

        public void UndoChanges()
        {
            ChangeNotificationEnabled = true;

            var type = this.GetType();
            
            foreach (var dirtyState in _dirtyStates)
            {
                if (dirtyState.Value.IsDirty)
                {
                    var propertyName = dirtyState.Key;
                    var originalValue = dirtyState.Value.OriginalValue;

                    // Sæt den ændrede property tilbage til dens oprindelige værdi.
                    // Dette vil også indirekte kalde "UpdateDirtyState" med den oprindelige værdi,
                    // så property'ens state ikke længere vil være dirty. (Current bliver sat til original).
                    type.GetProperty(propertyName).SetValue(this, originalValue);
                }
            }
        }

        private void RaiseDirtyStateChanged()
        {
            DirtyStateChanged();
        }

        #endregion


        #region Helpers

        private bool _isReevaluatingOnDirtyStateAndValidationChanges;

        public void ReevaluateOnDirtyStateAndValidationChanges(params Command[] commands)
        {
            if (_isReevaluatingOnDirtyStateAndValidationChanges)
                return;

            _isReevaluatingOnDirtyStateAndValidationChanges = true;

            this.DirtyStateChanged += delegate { ReevaluateCommands(commands); };
            this.ErrorsChanged += delegate { ReevaluateCommands(commands); };
        }

        private void ReevaluateCommands(IEnumerable<Command> commands)
        {
            if (commands == null)
                return;

            foreach (var command in commands)
                command.RaiseCanExecuteChanged();
        }

        public bool CanSave
        {
            get { return IsDirty && !HasErrors; }
        }

        #endregion

    }
}
