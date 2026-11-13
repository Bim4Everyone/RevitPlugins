using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Warnings;
using RevitEnumerateBySpline.Views;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Управляет окнами и запросами выбора.</summary>
internal interface IWindowService {

    public void HideMainWindow();
    public void ShowMainWindow();
    public void CloseMainWindow();

}
