using System;
using System.Windows.Input;

namespace DeveloperTest.Model
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> executeAction;
        private readonly Func<object, bool> canExectuteAction;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> executeAction, Func<object, bool> canExectuteAction)
        {
            this.executeAction = executeAction;
            this.canExectuteAction = canExectuteAction;
        }

        public bool CanExecute(object parameter)
        {
            if (canExectuteAction != null)
            {
                return canExectuteAction.Invoke(parameter);
            }

            return false;
        }

        public void Execute(object parameter)
        {
            executeAction?.Invoke(parameter);
        }
    }
}
