using System;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.Commands
{
    public sealed class RelayCommand : Command
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            Guard.AgainstNull(() => execute);

            _execute = execute;
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            CheckParameter(parameter);
            return _canExecute == null || _canExecute();
        }

        public override void Execute(object parameter)
        {
            CheckParameter(parameter);
            _execute();
        }

        private void CheckParameter(object parameter)
        {
            if (parameter != null)
                throw new InvalidOperationException("Parameter not expected. Use ParameterizedRelayCommand instead.");
        }
    }
}
