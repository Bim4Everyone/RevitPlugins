using System;
using System.Threading.Tasks;

namespace dosymep.WPF.Commands {
    internal class AsyncRelayCommand : AsyncRelayCommand<object> {
        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute = null)
            : base(execute, canExecute) {
        }
    }
}