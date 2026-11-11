using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Warnings;
using RevitEnumerateBySpline.Views;

namespace RevitEnumerateBySpline.Models.Interfaces;

internal interface IWindowService {
    /// <summary>Устанавливает текущее главное окно.</summary>
    void SetMainWindow(MainWindow mainWindow);

    /// <summary>Закрывает окно для выбора линий в Revit.</summary>
    void RequestCurveSelection();

    /// <summary>Передаёт предупреждения для показа после закрытия главного окна.</summary>
    void RequestWarnings(IReadOnlyCollection<WarningElement> warnings);

    /// <summary>Показывает окно с предупреждениями о помещениях.</summary>
    void ShowWarnings();
}
