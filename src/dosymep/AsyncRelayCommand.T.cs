using System;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace dosymep.WPF.Commands {
    internal class AsyncRelayCommand<T> : ObservableObject, IAsyncCommand<T> {
        private Func<T, Task> execute;
        private Func<T, bool> canExecute;
        private bool _isExecuting;

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute = null) {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool IsExecuting {
            get => _isExecuting;
            set => this.RaiseAndSetIfChanged(ref _isExecuting, value);
        }

        bool ICommand.CanExecute(object parameter) {
            return CanExecute((T) parameter);
        }

        async void ICommand.Execute(object parameter) {
            await ExecuteAsync((T) parameter);
        }

        public bool CanExecute(T parameter) {
            return !IsExecuting && (this.canExecute == null || this.canExecute(parameter));
        }

        public async Task ExecuteAsync(T parameter) {
            IsExecuting = true;
            try {
                await this.execute((T) parameter);
            } catch(OperationCanceledException) {
                await GetPlatformService<INotificationService>()
                    .CreateWarningNotification("C#", "Отмена выполнения команды.")
                    .ShowAsync();
            } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                await GetPlatformService<INotificationService>()
                    .CreateWarningNotification("C#", "Отмена выполнения команды.")
                    .ShowAsync();
            } catch(Exception ex) {
                await GetPlatformService<INotificationService>()
                    .CreateFatalNotification("C#", "Ошибка выполнения команды.")
                    .ShowAsync();

                GetPlatformService<ILoggerService>()
                    .Warning(ex, "Ошибка выполнения команды.");

#if DEBUG
                TaskDialog.Show("Ошибка!", ex.ToString());
#else
                TaskDialog.Show("Ошибка!", ex.Message);
#endif
            } finally {
                IsExecuting = false;
            }
        }

        protected TService GetPlatformService<TService>() {
            return ServicesProvider.GetPlatformService<TService>();
        }
    }
}