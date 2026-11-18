using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;

namespace RevitLintelsManager.Services;

internal class SettingsCommandFactory(IMessageBoxService messages, ILoggerService logger,
    ILocalizationService localization) {
    public ICommand Create(Action execute, Func<bool> canExecute = null) {
        return new SettingsCommand(_ => Execute(execute), _ => canExecute?.Invoke() ?? true);
    }

    public ICommand Create<T>(Action<T> execute, Func<T, bool> canExecute = null) {
        return new SettingsCommand(parameter => Execute(() => execute((T) parameter)),
            parameter => parameter is T value && (canExecute?.Invoke(value) ?? true));
    }

    public ICommand CreateAsync(Func<Task> execute, Func<bool> canExecute = null) {
        return new SettingsCommand(_ => ExecuteAsync(execute), _ => canExecute?.Invoke() ?? true);
    }

    public ICommand CreateAsync<T>(Func<T, Task> execute, Func<T, bool> canExecute = null) {
        return new SettingsCommand(parameter => ExecuteAsync(() => execute((T) parameter)),
            parameter => parameter is T value && (canExecute?.Invoke(value) ?? true));
    }

    private void Execute(Action execute) {
        try {
            execute();
        } catch(OperationCanceledException) {
            // Отмена операции не требует глобального уведомления платформы.
        } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            // Отмена выбора в Revit.
        } catch(Exception exception) {
            ShowError(exception);
        }
    }

    private async Task ExecuteAsync(Func<Task> execute) {
        try {
            await execute();
        } catch(OperationCanceledException) {
            // Отмена операции не требует глобального уведомления платформы.
        } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            // Отмена выбора в Revit.
        } catch(Exception exception) {
            ShowError(exception);
        }
    }

    private void ShowError(Exception exception) {
        string title = localization.GetLocalizedString("Common.Error");
        logger.Warning(exception, title);
        messages.Show(exception.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    // Execute команд платформы зависит от RevitAPIUI и глобальных уведомлений.
    // Окно настроек использует только сервисы своего контейнера.
    private sealed class SettingsCommand : ICommand {
        private readonly Action<object> _execute;
        private readonly Func<object, Task> _executeAsync;
        private readonly Func<object, bool> _canExecute;
        private bool _isExecuting;

        public SettingsCommand(Action<object> execute, Func<object, bool> canExecute) {
            _execute = execute;
            _canExecute = canExecute;
        }

        public SettingsCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute) {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) {
            return !_isExecuting && _canExecute(parameter);
        }

        public async void Execute(object parameter) {
            if(!CanExecute(parameter)) {
                return;
            }
            if(_executeAsync is null) {
                _execute(parameter);
                return;
            }
            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();
            try {
                await _executeAsync(parameter);
            } finally {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
