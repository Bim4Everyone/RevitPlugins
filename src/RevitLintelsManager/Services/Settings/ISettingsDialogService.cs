using System;
using System.Threading.Tasks;

namespace RevitLintelsManager.Services;

/// <summary>Контракт SettingsDialogService.</summary>
internal interface ISettingsDialogService {

    /// <summary>Открывает модальный диалог ввода имени.</summary>
    /// <param name="title">Заголовок.</param>
    /// <param name="initialName">Начальное имя.</param>
    /// <param name="validate">Проверка имени.</param>
    /// <returns>Имя или null при отмене.</returns>
    Task<string> RequestNameAsync(string title, string initialName, Func<string, bool> validate);
}
