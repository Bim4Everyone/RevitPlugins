using System;
using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Warnings;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Управляет окнами и запросами выбора.</summary>
internal interface IWindowService {
    /// <summary>
    /// Скрывает главное окно и откладывает выбор элементов до выхода из текущего модального диалога.
    /// </summary>
    /// <param name="select">Действие выбора элементов в Revit.</param>
    /// <remarks>После выполнения действия главное окно показывается повторно.</remarks>
    void RequestSelection(Action select);

    /// <summary>
    /// Показывает главное окно модально и обрабатывает запросы выбора элементов до завершения диалога.
    /// </summary>
    /// <returns>
    /// Итоговый результат диалога: <see langword="true"/> при подтверждении,
    /// <see langword="false"/> при отмене или <see langword="null"/>, если результат не задан.
    /// </returns>
    bool? ShowMainWindow();

    /// <summary>
    /// Устанавливает результат главного диалога для завершения его работы.
    /// </summary>
    /// <param name="dialogResult">
    /// <see langword="true"/> для подтверждения или <see langword="false"/> для отмены.
    /// </param>
    void CloseMainWindow(bool dialogResult);

    /// <summary>
    /// Показывает немодальное окно предупреждений о помещениях.
    /// </summary>
    /// <param name="warningElements">Предупреждения, которые нужно отобразить.</param>
    void ShowWarningWindow(IReadOnlyCollection<WarningElement> warningElements);
}
