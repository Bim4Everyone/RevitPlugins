using System;
using System.Windows.Input;

using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace dosymep.WPF.Commands {
    internal class RelayCommand<T> : ICommand<T> {
        private Action<T> execute;
        private Func<T, bool> canExecute;

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null) {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        bool ICommand.CanExecute(object parameter) {
            return CanExecute((T) parameter);
        }

        void ICommand.Execute(object parameter) {
            Execute((T) parameter);
        }

        public bool CanExecute(T parameter) {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(T parameter) {
            try {
                this.execute((T) parameter);
            } catch(OperationCanceledException) {
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification("C#", "Отмена выполнения команды.")
                    .ShowAsync();
            } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification("C#", "Отмена выполнения команды.")
                    .ShowAsync();
            } catch(Exception ex) {
                GetPlatformService<INotificationService>()
                    .CreateFatalNotification("C#", "Ошибка выполнения команды.")
                    .ShowAsync();

                GetPlatformService<ILoggerService>()
                    .Warning(ex, "Ошибка выполнения команды.");

#if DEBUG
                TaskDialog.Show("Ошибка!", ex.ToString());
#else
                TaskDialog.Show("Ошибка!", ex.Message);
#endif
            }
        }

        protected TService GetPlatformService<TService>() {
            return ServicesProvider.GetPlatformService<TService>();
        }
    }
}