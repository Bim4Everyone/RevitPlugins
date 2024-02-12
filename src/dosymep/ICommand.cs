using System.Windows.Input;

namespace dosymep.WPF.Commands {
    internal interface ICommand<T> : ICommand {
        void Execute(T parameter);
        bool CanExecute(T parameter);
    }
}