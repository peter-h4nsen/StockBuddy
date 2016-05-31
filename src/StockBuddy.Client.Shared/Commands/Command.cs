using System;
using System.Windows.Input;

namespace StockBuddy.Client.Shared.Commands
{
    public abstract class Command : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, new EventArgs());
        }

        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);
    }
}
