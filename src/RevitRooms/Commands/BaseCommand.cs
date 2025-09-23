using System;
using System.Windows.Input;

namespace RevitRooms.Commands;
internal abstract class BaseCommand : ICommand {
    public event EventHandler CanExecuteChanged {
        add => CommandManager.RequerySuggested += value; 
        remove => CommandManager.RequerySuggested -= value;
    }

    public abstract void Execute(object parameter);

    public bool CanExecute(object parameter) {
        return true;
    }
}
