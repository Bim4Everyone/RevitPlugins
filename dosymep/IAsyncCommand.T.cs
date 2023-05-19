using System.Threading.Tasks;

namespace dosymep.WPF.Commands {
    internal interface IAsyncCommand<T> : IAsyncCommand {
        bool CanExecute(T parameter);
        Task ExecuteAsync(T parameter);
    }
}