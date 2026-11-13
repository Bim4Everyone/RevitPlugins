using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Warnings;
using RevitEnumerateBySpline.Views;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Управляет окнами и запросами выбора.</summary>
internal interface IWindowService {
    void HideMainWindow();
    void ShowMainWindow();
    void CloseMainWindow();
    void ShowWarningWindow(IReadOnlyCollection<WarningElement> warningElements);

}
