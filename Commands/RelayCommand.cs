using System;
using System.Windows.Input;

namespace HeroDrafter.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Action<object> _executeWithParam;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Action<object> execute, Func<bool, bool> canExecute = null)
        {
            _executeWithParam = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null) return _canExecute();
            return true;
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
                _execute();
            else if (_executeWithParam != null)
                _executeWithParam(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}