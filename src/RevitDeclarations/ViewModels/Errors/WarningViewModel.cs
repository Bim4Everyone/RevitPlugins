using System.Collections.Generic;
using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitDeclarations.ViewModels;
internal class WarningViewModel : BaseViewModel {
    public string WarningType { get; set; }
    public string Description { get; set; }
    public string DocumentName { get; set; }
    public string FullName => $"{WarningType} в проекте \"{DocumentName}\"";

    public IList<WarningElementViewModel> Elements { get; set; } = [];
}

/// <summary>
/// Класс для представления элемента Revit с ошибкой.
/// Содержит информацию об этом элементе.
/// </summary>
public class WarningElementViewModel {
    public WarningElementViewModel(string elementInfo, string errorInfo) {
        ElementInfo = elementInfo;
        WarningInfo = errorInfo;
    }

    public string ElementInfo { get; }
    public string WarningInfo { get; }
}
