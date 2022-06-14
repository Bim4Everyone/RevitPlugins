using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace dosymep.WPF.Commands {
    internal class RelayCommand : ICommand {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null) {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter) {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter) {
            try {
                this.execute(parameter);
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

#if D2020 || D2021 || D2022
                TaskDialog.Show("Ошибка!", ex.ToString());
#else
                TaskDialog.Show("Ошибка!", ex.ToString());
#endif
            }
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}