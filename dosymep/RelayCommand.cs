using System;
using System.Threading.Tasks;

namespace dosymep.WPF.Commands {
    internal class RelayCommand : RelayCommand<object> {
        public RelayCommand Create(Action execute, Func<bool> canExecute = null) {
            return new RelayCommand(p => execute(), canExecute == null ? (Func<object, bool>) null : p => canExecute());
        }

        public RelayCommand<T> Create<T>(Action<T> execute, Func<T, bool> canExecute = null) {
            return new RelayCommand<T>(execute, canExecute);
        }
        
        public AsyncRelayCommand CreateAsync(Func<Task> execute, Func<bool> canExecute = null) {
            return new AsyncRelayCommand(p => execute(), canExecute == null ? (Func<object, bool>) null : p => canExecute());
        }

        public AsyncRelayCommand<T> CreateAsync<T>(Func<T, Task> execute, Func<T, bool> canExecute = null) {
            return new AsyncRelayCommand<T>(execute, canExecute);
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
            : base(execute, canExecute) {
        }
    }
}