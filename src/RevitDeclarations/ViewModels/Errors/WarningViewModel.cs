using System.Collections.Generic;
using System.Collections.ObjectModel;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitDeclarations.ViewModels;
internal class WarningViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;

    public WarningViewModel(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public string WarningType { get; set; }
    public string Description { get; set; }
    public string DocumentName { get; set; }
    public string FullName => 
        _localizationService.GetLocalizedString("WarningsWindow.WarningFullName", WarningType, DocumentName);

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
