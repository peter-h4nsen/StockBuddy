using System;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.Commands
{
    public sealed class ParameterizedRelayCommand<T> : Command
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;

        public ParameterizedRelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public ParameterizedRelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            Guard.AgainstNull(() => execute);

            _execute = execute;
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public override void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
