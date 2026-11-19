using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RevitLintelsManager.Services;

/// <summary>Контракт SettingsCommandFactory.</summary>
internal interface ISettingsCommandFactory {

    /// <summary>Создаёт команду с обработкой ошибок.</summary>
    /// <param name="execute">Действие.</param>
    /// <param name="canExecute">Проверка доступности.</param>
    /// <returns>Команда интерфейса.</returns>
    ICommand Create(Action execute, Func<bool> canExecute = null);

    /// <summary>Создаёт команду с параметром и обработкой ошибок.</summary>
    /// <param name="execute">Действие.</param>
    /// <param name="canExecute">Проверка доступности.</param>
    /// <returns>Команда интерфейса.</returns>
    ICommand Create<T>(Action<T> execute, Func<T, bool> canExecute = null);

    /// <summary>Создаёт асинхронную команду с защитой от повторного вызова.</summary>
    /// <param name="execute">Действие.</param>
    /// <param name="canExecute">Проверка доступности.</param>
    /// <returns>Команда интерфейса.</returns>
    ICommand CreateAsync(Func<Task> execute, Func<bool> canExecute = null);

    /// <summary>Создаёт асинхронную команду с параметром.</summary>
    /// <param name="execute">Действие.</param>
    /// <param name="canExecute">Проверка доступности.</param>
    /// <returns>Команда интерфейса.</returns>
    ICommand CreateAsync<T>(Func<T, Task> execute, Func<T, bool> canExecute = null);
}
